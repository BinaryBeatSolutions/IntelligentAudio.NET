
using NAudio.Wave;

namespace IntelligentAudio.Infrastructure.Audio;

public class MicrophoneSource(
    IAudioStreamSource audioSource,
    ILogger<MicrophoneSource> _logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        _logger.LogInformation("[IntelligentAudio.NET] Hardware Source Starting...");

        if (audioSource is WindowsAudioSource winSource)
        {
            winSource.Start();
        }

        // Vi väntar bara på att tjänsten ska avslutas. 
        // Låt IntelligentAudioEngine hantera ReadAllAsync()!
        await Task.Delay(Timeout.Infinite, ct);
    }
}