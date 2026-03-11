using IntelligentAudio.Contracts;
using IntelligentAudio.Contracts.Interfaces;
using Microsoft.Extensions.Logging;

namespace IntelligentAudio.NeuralEngine.Handlers;

public sealed class NeuralIntentHandlerImpl(ILogger<NeuralIntentHandlerImpl> logger, ChannelWriter<string> neuralChannel) : IIntentHandler
{
    // Vi låter Neural Engine försöka hantera allt som Whisper spottar ur sig.
    // AI-motorn (ONNX) avgör själv via 'threshold' om det var ett giltigt kommando.
    public bool CanHandle(string text) => !string.IsNullOrWhiteSpace(text);

    public async ValueTask HandleAsync(string text, CancellationToken ct)
    {
        logger.LogInformation("[NeuralHandler] Pushing text to Engine: {text}", text); // <-- Lägg till denna!
        if (!neuralChannel.TryWrite(text)) 
            await neuralChannel.WriteAsync(text, ct);
    }
}