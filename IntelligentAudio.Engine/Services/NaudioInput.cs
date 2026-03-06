
using NAudio.Wave;

namespace IntelligentAudio.Engine.Services;

public class NaudioInput : IAudioInput
{
    // Markera som readonly - den sätts i konstruktorn och ändras aldrig
    private readonly WaveInEvent _waveIn;

    public event Action<Span<float>>? DataAvailable;

    // Eftersom _waveIn inte är nullable, är detta 100% säkert
    public int SampleRate => _waveIn.WaveFormat.SampleRate;

    public NaudioInput(int requestedRate = 44100)
    {
        // Initiera direkt
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(requestedRate, 1)
        };

        _waveIn.DataAvailable += OnWaveInDataAvailable;
    }

    private void OnWaveInDataAvailable(object? sender, WaveInEventArgs e)
    {
     
        //DataAvailable?.Invoke(floatBuffer);
    }
}