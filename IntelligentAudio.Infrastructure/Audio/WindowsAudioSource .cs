
using NAudio.Wave;

namespace IntelligentAudio.Infrastructure.Audio;

public sealed class WindowsAudioSource : IAudioStreamSource, IDisposable
{
    private readonly Channel<float[]> _channel;
    private WaveInEvent? _waveIn;
    private readonly IAudioBufferProvider _bufferProvider;
    private bool _isDisposed;
    private readonly ArrayPool<float> _pool = ArrayPool<float>.Shared;
    private readonly NoiseGateProcessor _noiseGate; 
    private readonly SimpleHighPassFilter _filter;
    private readonly ILogger<WindowsAudioSource> _logger; //<--- debug, to be removed.
    public ChannelReader<float[]> AudioStream => _channel.Reader;

    public bool IsRecording { get; private set; } = false;

    public WindowsAudioSource(ILogger<WindowsAudioSource> logger, IAudioBufferProvider bufferProvider, NoiseGateProcessor noiseGate, SimpleHighPassFilter filter)
    {
        _bufferProvider = bufferProvider;
        _noiseGate = noiseGate;
        _filter = filter;
        _logger = logger;

        _channel = Channel.CreateBounded<float[]>(new BoundedChannelOptions(10)
        {
            FullMode = BoundedChannelFullMode.DropOldest
        });
    }

    public void Start(int deviceNumber = 0)
    {
        if (IsRecording) return;

        // 2. Setup WaveIn
        _waveIn = new WaveInEvent
        {
            DeviceNumber = deviceNumber,
            WaveFormat = new WaveFormat(44100, 16, 1),
            BufferMilliseconds = 100
        };

        // 3. Prenumerera på eventet (Använd en lokal referens till kanalen för säkerhet)
        var writer = _channel.Writer;

        _waveIn.DataAvailable += (s, e) =>
        {
            if (_isDisposed || e.BytesRecorded <= 0) return;

            // 1. Cast raw bytes to shorts (Zero-copy)
            var rawShorts = MemoryMarshal.Cast<byte, short>(e.Buffer.AsSpan(0, e.BytesRecorded));

            // 2. Check NoiseGate (using our new logic)
            if (_noiseGate.IsOpen(rawShorts))
            {
                _logger.LogInformation("[WindowsAudioSource] {0}, {rawShorts}", _noiseGate.IsOpen(rawShorts), rawShorts.ToString());

                int sampleCount = rawShorts.Length;
                int targetCount = (int)(sampleCount * (16000.0 / 44100.0));

                float[] rawBuffer = _pool.Rent(sampleCount);
                float[] resampledBuffer = _pool.Rent(targetCount);

                try
                {
                    var rawSpan = rawBuffer.AsSpan(0, sampleCount);
                    var destSpan = resampledBuffer.AsSpan(0, targetCount);

                    // 4-6. Processing (högpresterande via Span)
                    for (int i = 0; i < sampleCount; i++) rawSpan[i] = rawShorts[i] / 32768f;
                    _filter.Process(rawSpan);
                    _bufferProvider.ProcessResampling(rawSpan, destSpan);

                    // 7. Handover (Noll allokering eftersom AudioSegment är en readonly record struct)
                    var segment = new AudioSegment(resampledBuffer, targetCount);

                    if (!_channel.Writer.TryWrite(segment.Buffer))
                    {
                        _logger.LogInformation("[WindowsAudioSource] {0}",  resampledBuffer.ToString());
                        _pool.Return(resampledBuffer); // Pipeline full, lämna tillbaka
                    }
                    // NOTERA: Ingen _pool.Return(resampledBuffer) här vid framgång! 
                    // Ägarskapet har flyttats till konsumenten av kanalen.
                }
                catch
                {
                    // Logg
                    _pool.Return(resampledBuffer);
                }
                finally
                {
                    // Den temporära 44.1kHz bufferten behövs aldrig efter detta block
                    _pool.Return(rawBuffer);
                }
            }
        };

        _waveIn.StartRecording();
        IsRecording = true;
    }

    public void Dispose()
    {
        _isDisposed = true;
        _waveIn?.StopRecording();
        _waveIn?.Dispose();
    }
}

