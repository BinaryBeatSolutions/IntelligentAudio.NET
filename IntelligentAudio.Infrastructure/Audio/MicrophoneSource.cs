
using NAudio.Wave;

namespace IntelligentAudio.Infrastructure.Audio;

public class MicrophoneSource(AudioPipeline pipeline, ILogger<MicrophoneSource> logger) : BackgroundService
{
    private WaveInEvent? _waveIn;

    protected override Task ExecuteAsync(CancellationToken ct)
    {
        _waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(44100, 16, 1) // Standard mono 44.1kHz
        };

        _waveIn.DataAvailable += (s, e) =>
        {
            // Konvertera 16-bit PCM till float[] för vår pipeline
            var samples = new float[e.BytesRecorded / 2];
            for (int i = 0; i < samples.Length; i++)
            {
                short sample = (short)((e.Buffer[i * 2 + 1] << 8) | e.Buffer[i * 2]);
                samples[i] = sample / 32768f;
            }

            // Skicka in i "röret"
            pipeline.Writer.TryWrite(samples);
        };

        _waveIn.StartRecording();
        logger.LogInformation("Mikrofoninspelning startad...");

        // Håll tjänsten vid liv tills appen stängs
        return Task.Delay(-1, ct);
    }

    public override void Dispose()
    {
        _waveIn?.StopRecording();
        _waveIn?.Dispose();
        base.Dispose();
    }
}