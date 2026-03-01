

using NAudio.Wave;
using static System.Net.WebRequestMethods;

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

    public ChannelReader<float[]> AudioStream => _channel.Reader;
    public bool IsRecording { get; private set; }

    public WindowsAudioSource(IAudioBufferProvider bufferProvider, NoiseGateProcessor noiseGate, SimpleHighPassFilter filter)
    {
        _bufferProvider = bufferProvider;
        _noiseGate = noiseGate;
        _filter = filter;

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
            // Hyr en buffer, fyll med brus och skicka direkt
            float[] debugBuffer = ArrayPool<float>.Shared.Rent(1000);
            for (int i = 0; i < 1000; i++) debugBuffer[i] = 0.5f;

            if (!_channel.Writer.TryWrite(debugBuffer))
            {
                ArrayPool<float>.Shared.Return(debugBuffer);
            }
        };


        //_waveIn.DataAvailable += (s, e) =>
        //{
        //    if (_isDisposed || e.BytesRecorded <= 0) return;

        //    // 1. Cast raw bytes to shorts (Zero-copy)
        //    var rawShorts = MemoryMarshal.Cast<byte, short>(e.Buffer.AsSpan(0, e.BytesRecorded));

        //    // 2. Check NoiseGate (using our new logic)
        //    if (_noiseGate.IsOpen(rawShorts))
        //    {
        //        int sampleCount = rawShorts.Length;
        //        int targetCount = (int)(sampleCount * (16000.0 / 44100.0));

        //        // 3. Rent buffers from Pool
        //        float[] rawBuffer = _pool.Rent(sampleCount);
        //        float[] resampledBuffer = _pool.Rent(targetCount);

        //        try
        //        {
        //            var rawSpan = rawBuffer.AsSpan(0, sampleCount);
        //            var destSpan = resampledBuffer.AsSpan(0, targetCount);

        //            // 4. Convert short -> float (Normalization)
        //            for (int i = 0; i < sampleCount; i++)
        //                rawSpan[i] = rawShorts[i] / 32768f;

        //            // 5. Apply HighPassFilter (Clean up DC offset and rumble)
        //            _filter.Process(rawSpan);

        //            // 6. Resample to 16kHz for Whisper AI
        //            _bufferProvider.ProcessResampling(rawSpan, destSpan);

        //            // 7. Write to Channel
        //            if (!_channel.Writer.TryWrite(resampledBuffer))
        //            {
        //                // If channel is full, we must return it immediately
        //                _pool.Return(resampledBuffer);
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            // Fail safe: return if we crash
        //            _pool.Return(resampledBuffer);
        //        }
        //        finally
        //        {
        //            // Always return the temporary 44.1kHz buffer
        //            _pool.Return(rawBuffer);
        //        }
        //    }
        //};

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

