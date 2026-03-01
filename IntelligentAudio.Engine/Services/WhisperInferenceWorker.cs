

using Whisper.net;

namespace IntelligentAudio.Engine.Services;

public sealed class WhisperInferenceWorker(
    ILogger<WhisperInferenceWorker> logger,
    DefaultWhisperModelService modelService, // Vår loader
    AudioPipeline pipeline) : BackgroundService
{
    private const int BufferCountForInference = 10; // Samla ca 1 sek (10 * 100ms)

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await modelService.EnsureModelReadyAsync(WhisperModelType.Base, ct);
        using var processor = modelService.CreateProcessor();
        

        // Vi hyr EN stor buffert för hela livslängden (3 sekunders utrymme)
        float[] sessionBuffer = ArrayPool<float>.Shared.Rent(16000 * 3);
        int currentPos = 0;

        try
        {
            // Denna loop är motorns hjärta
            await foreach (var buffer in pipeline.Reader.ReadAllAsync(ct))
            {
                try
                {
                    // 1. Kopiera in småpaketet i vår fasta sessionBuffer (Zero Allocation)
                    int toCopy = Math.Min(buffer.Length, sessionBuffer.Length - currentPos);
                    buffer.AsSpan(0, toCopy).CopyTo(sessionBuffer.AsSpan(currentPos));
                    currentPos += toCopy;

                    // 2. Har vi samlat 1 sekund? (16000 samples)
                    if (currentPos >= 16000)
                    {
                        
                        // 3. Kör Whisper på den exakta delen av bufferten
                        await foreach (var result in processor.ProcessAsync(sessionBuffer.AsMemory(0, currentPos), ct))
                        {
                           
                            if (!string.IsNullOrWhiteSpace(result.Text))
                                logger.LogInformation("[AI] Intent detected: {text}", result.Text.Trim());
                        }
                        currentPos = 0; // "Reset" utan att kasta bort arrayen
                    }
                }
                finally
                {
                    // 4. Returnera det lilla paketet OMEDELBART till poolen
                    ArrayPool<float>.Shared.Return(buffer);
                }
            }
        }
        finally
        {
            ArrayPool<float>.Shared.Return(sessionBuffer);
        }
    }
}
