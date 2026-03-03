

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
        using var processor = aiService.CreateProcessor(); //Create AI engine Whisper.NET processor.

        const int sessionSize = 48000; // 3 sekunder @ 16kHz
        float[] sessionBuffer = _pool.Rent(sessionSize);
        int currentPos = 0;

        logger.LogInformation("[Engine] Inference Loop Started. Listening...");

        try
        {
            logger.LogInformation("[Engine] Start loop");

            // DENNA LOOP körs varje gång ett nytt ljudpaket kommer från mikrofonen
            await foreach (var segment in pipeline.Reader.ReadAllAsync(ct))
            {
                logger.LogDebug("Received segment: {len}", segment.Length);


                // 1. KOPIERA in ljudet i vår stora 3-sekunders hink
                int toCopy = Math.Min(segment.Length, sessionSize - currentPos);
                segment.Buffer.AsSpan(0, toCopy).CopyTo(sessionBuffer.AsSpan(currentPos));

                // 2. ÖKA POSITIONEN (Nu blir currentPos större!)
                currentPos += toCopy;

                // 3. KOLLA OM HINKEN ÄR FULL (Här inne sker magin)
                if (currentPos >= sessionSize)
                {
                    // Nu skickar vi 3 sekunder till AI:n
                    Console.WriteLine($"[DEBUG] Executing AI on {currentPos} samples...");

                    await foreach (var result in processor.ProcessAsync(sessionBuffer.AsMemory(0, currentPos), ct))
                    {
                        if (!string.IsNullOrWhiteSpace(result.Text))
                        {
                            Console.WriteLine($"[WHISPER RAW]: {result.Text}");
                        }
                    }

                    // 4. NOLLSTÄLL för nästa 3 sekunder
                    currentPos = 0;
                }

                // Returnera alltid det lilla segmentet till poolen direkt
                _pool.Return(segment.Buffer);
            }
        }
        finally
        {
            logger.LogInformation("[Engine] finally");
            _pool.Return(sessionBuffer);
        }
    }
}
