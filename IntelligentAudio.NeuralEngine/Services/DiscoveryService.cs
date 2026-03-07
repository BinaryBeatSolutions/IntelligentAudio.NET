
using BuildSoft.OscCore;

namespace IntelligentAudio.NeuralEngine.Services;

public sealed class DiscoveryService : IDisposable
{
    private readonly DefaultNeuralTokenizerImpl _tokenizer;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly NeuralParameterRegistry _registry;
    private readonly float[] _tempEmbeddingBuffer;

    public DiscoveryService(
        DefaultNeuralTokenizerImpl tokenizer,
        IEmbeddingGenerator embeddingGenerator,
        NeuralParameterRegistry registry)
    {
        _tokenizer = tokenizer;
        _embeddingGenerator = embeddingGenerator;
        _registry = registry;
        _tempEmbeddingBuffer = new float[embeddingGenerator.VectorSize];
    }

    // STRICT: Denna metod anropas när OSC-servern tar emot en lista med parametrar
    public void OnDeviceParametersReceived(OscMessageValues message)
    {
        // 1. Rensa existerande cache för den gamla pluginen
        _registry.Dispose();

        // 2. Loopa igenom OSC-meddelandet (STRICT: Ingen LINQ)
        // Vi antar att Ableton skickar par: [Index, Namn, Index, Namn...]
        for (int i = 0; i < message.ElementCount; i += 2)
        {
            int paramId = message.ReadIntElement(i);
            string paramName = message.ReadStringElement(i + 1);

            // 3. Tokenize på stacken
            Span<long> tokens = stackalloc long[128];
            int tokenCount = _tokenizer.TokenizeToSpan(paramName, tokens);

            // 4. Skapa embedding till temporär buffer
            _embeddingGenerator.GenerateEmbedding(tokens.Slice(0, tokenCount), _tempEmbeddingBuffer);

            // 5. Registrera i det linjära minnet (Blixtsnabb sökning redo!)
            _registry.RegisterParameter(paramId, _tempEmbeddingBuffer);
        }

        // Nu är motorn "armerad" för just denna VST/Plugin!
    }

    public void Dispose() => _registry.Dispose();
}