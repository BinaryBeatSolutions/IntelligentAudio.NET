
using NAudio.Wave;

namespace IntelligentAudio.Infrastructure.Audio;

public class MicrophoneSource(
    IAudioStreamSource audioSource,
    ILogger<MicrophoneSource> _logger, 
    IAudioBufferProvider bufferProvider) : BackgroundService
{

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("[IntelligentAudio.NET] Input test initiated. Listening for audio...");

        // Starta hårdvaran (Windows/NAudio)
        if (audioSource is WindowsAudioSource winSource)
        {
            winSource.Start();
        }
        try
        {
            await foreach (var buffer in audioSource.AudioStream.ReadAllAsync(ct))
            {
                try
                {
                    // Float-metoden, resamplade 16kHz data
                    float rms = bufferProvider.CalculateRms(buffer.AsSpan());

                    if (rms > 0.04f) // Låg tröskel för att se att mätaren lever
                    {
                        // Enkel visuell mätare i konsolen
                        string bar = new string('=', (int)Math.Clamp(rms * 100, 0, 50));
                        Console.WriteLine($"[RMS: {rms:F4}] {bar}");
                    }
                }
                finally
                {
                    ArrayPool<float>.Shared.Return(buffer);
                }
            }
        }
        catch (OperationCanceledException) { /* Normal shutdown */ }
    }
}