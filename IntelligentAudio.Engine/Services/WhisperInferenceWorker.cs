

using Whisper.net;

namespace IntelligentAudio.Engine.Services;

public sealed class WhisperInferenceWorker(
    DefaultWhisperModelService aiService,
    AudioPipeline pipeline,
    ILogger<WhisperInferenceWorker> logger) : BackgroundService
{
    private readonly ArrayPool<float> _pool = ArrayPool<float>.Shared;

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await aiService.EnsureModelReadyAsync(WhisperModelType.Tiny, ct);
        using var processor = aiService.CreateProcessor();

        const int sessionSize = 48000; // 3 sekunder @ 16kHz
        float[] sessionBuffer = _pool.Rent(sessionSize);
        int currentPos = 0;

        try
        {
            await foreach (var segment in pipeline.Reader.ReadAllAsync(ct))
            {
                // 1. Kopiera in i 3-sekunders-hinken
                int toCopy = Math.Min(segment.Length, sessionSize - currentPos);
                Array.Copy(segment, 0, sessionBuffer, currentPos, toCopy);
                currentPos += toCopy;

                // 2. Lämna tillbaka segmentet till poolen (Viktigt: efter kopiering!)
                _pool.Return(segment);

                // 3. Har vi fyllt 3 sekunder?
                if (currentPos >= sessionSize)
                {
                    logger.LogInformation("[Engine] 3s Buffer Full. Sending to Whisper...");

                    // Skicka EXAKT sessionSize samples till Whisper
                    var audioView = sessionBuffer.AsMemory(0, sessionSize);

                    await foreach (var result in processor.ProcessAsync(audioView, ct))
                    {
                        if (!string.IsNullOrWhiteSpace(result.Text))
                        {
                            // HÄR SKALL SVARET KOMMA
                            Console.WriteLine($"[WHISPER RAW]: {result.Text.Trim()}");
                        }
                    }

                    // 4. Nollställ positionen för nästa 3 sekunder
                    currentPos = 0;
                    // Valfritt: Rensa bufferten för att undvika "eko" från förra vändan
                    Array.Clear(sessionBuffer, 0, sessionSize);
                }
            }
        }
        finally
        {
            _pool.Return(sessionBuffer);
        }
    }
}
