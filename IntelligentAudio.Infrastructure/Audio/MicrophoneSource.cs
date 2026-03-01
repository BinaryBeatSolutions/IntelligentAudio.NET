
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
                // DEBUG: Skrivs detta ut? Då vet vi att data flödar genom kanalen!
                // _logger.LogDebug("Received buffer with length: {length}", buffer.Length);

                float rms = bufferProvider.CalculateRms(buffer.AsSpan());

                if (rms > 0.04f) // Sänkt tröskel för test
                {
                    string bar = new string('=', (int)Math.Clamp(rms * 100, 0, 50));
                    //Console.WriteLine($"[RMS: {rms:F4}] {bar}");
                }

                ArrayPool<float>.Shared.Return(buffer);
            }
        }
        catch (OperationCanceledException) { /* Normal shutdown */ }
    }
}