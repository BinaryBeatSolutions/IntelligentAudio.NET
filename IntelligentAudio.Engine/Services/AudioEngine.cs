

namespace IntelligentAudio.Engine.Services;

public class AudioEngine(
    AudioPipeline pipeline,
    IEnumerable<IAudioProcessor> processors,
    IIntelligentAudioService aiService, // Whisper-tjänsten
    ILogger<AudioEngine> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Audio Engine startad. Väntar på ljud...");

        try
        {
            // Vi läser AudioSegment (Buffer + Length)
            await foreach (var segment in pipeline.Reader.ReadAllAsync(stoppingToken))
            {
                // Skapa en Span som bara täcker den faktiska ljuddatan
                Span<float> audioSpan = segment.AsSpan(0, segment.Length);

                // 1. Processa ljudet (HighPass, NoiseGate etc.)
                foreach (var processor in processors)
                {
                    processor.Process(audioSpan);
                }

                // 2. Skicka till AI (Whisper)
                // Vi måste VÄNTA (await) här, eller hantera pool-returen inuti tjänsten.
                // För lägst latens och stabilitet: await här.
                try
                {
                    await aiService.ProcessAudioAsync(segment, segment.Length, stoppingToken);
                    logger.LogInformation("aiService.ProcessAudioAsync");
                }
                finally
                {
                    // 3. NU lämnar vi tillbaka bufferten till poolen.
                    // Ingen annan får röra denna array efter denna rad!
                    ArrayPool<float>.Shared.Return(segment);
                }
            }
        }
        catch (OperationCanceledException) { /* Normal shutdown */ }
    }
}