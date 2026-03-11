
using NAudio.Wave;

namespace IntelligentAudio.Infrastructure.Audio;

public sealed class WindowsAudioSource : IAudioStreamSource, IDisposable
{
    private readonly AudioPipeline _pipeline; // Injiceras
    private WaveInEvent? _waveIn;
    private readonly IAudioBufferProvider _bufferProvider;
    private readonly NoiseGateProcessor _noiseGate;
    private readonly SimpleHighPassFilter _filter;
    private readonly ILogger<WindowsAudioSource> _logger;
    private readonly ArrayPool<float> _pool = ArrayPool<float>.Shared;
    private bool _isDisposed;

    // Implementera interfacet genom att peka på den delade pipelinens reader
    public ChannelReader<float[]> AudioStream => _pipeline.Reader;

    public bool IsRecording { get; private set; } = false;

    public WindowsAudioSource(
        ILogger<WindowsAudioSource> logger,
        IAudioBufferProvider bufferProvider,
        NoiseGateProcessor noiseGate,
        SimpleHighPassFilter filter,
        AudioPipeline pipeline) // Injicera här!
    {
        _logger = logger;
        _bufferProvider = bufferProvider;
        _noiseGate = noiseGate;
        _filter = filter;
        _pipeline = pipeline;
    }

    public void Start(int deviceNumber = 0)
    {
        if (IsRecording) return;

        _waveIn = new WaveInEvent
        {
            DeviceNumber = deviceNumber,
            WaveFormat = new WaveFormat(44100, 16, 1),
            BufferMilliseconds = 100
        };

        _waveIn.DataAvailable += (s, e) =>
        {
            if (_isDisposed || e.BytesRecorded <= 0) return;

            var rawShorts = MemoryMarshal.Cast<byte, short>(e.Buffer.AsSpan(0, e.BytesRecorded));

            if (_noiseGate.IsOpen(rawShorts))
            {
                int sampleCount = rawShorts.Length;
                int targetCount = (int)(sampleCount * (16000.0 / 44100.0));

                float[] rawBuffer = _pool.Rent(sampleCount);
                float[] resampledBuffer = _pool.Rent(targetCount);

                try
                {
                    var rawSpan = rawBuffer.AsSpan(0, sampleCount);
                    var destSpan = resampledBuffer.AsSpan(0, targetCount);

                    for (int i = 0; i < sampleCount; i++) rawSpan[i] = rawShorts[i] / 32768f;

                    _filter.Process(rawSpan);
                    _bufferProvider.ProcessResampling(rawSpan, destSpan);

                    // SKRIV DIREKT TILL DEN DELADE PIPELINEN
                    if (!_pipeline.Writer.TryWrite(resampledBuffer))
                    {
                        _pool.Return(resampledBuffer);
                    }
                }
                catch
                {
                    _pool.Return(resampledBuffer);
                }
                finally
                {
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

