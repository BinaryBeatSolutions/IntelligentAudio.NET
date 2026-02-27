

namespace IntelligentAudio.Engine.Services;

public class AudioEngine(
    AudioPipeline pipeline,
    IEnumerable<IAudioProcessor> processors,
    IIntelligentAudioService aiService, // Whisper-tjänsten
    ILogger<AudioEngine> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Audio Engine startad. Väntar på ljudström...");

        try
        {
            // Vi läser kontinuerligt från pipelinen (transportbandet)
            await foreach (var buffer in pipeline.Reader.ReadAllAsync(stoppingToken))
            {
                // 1. Processa ljudet (HighPass, NoiseGate etc.)
                // Vi använder Span för att arbeta direkt på bufferten utan kopior
                Span<float> audioSpan = buffer.AsSpan();

                foreach (var processor in processors)
                {
                    processor.Process(audioSpan);
                }

                // 2. Skicka det tvättade ljudet till AI:n
                // Vi gör detta asynkront så att vi inte blockerar nästa ljudpaket
                _ = aiService.ProcessAudioAsync(buffer, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Audio Engine stängs ner...");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ett kritiskt fel uppstod i Audio Engine");
        }
    }
}
