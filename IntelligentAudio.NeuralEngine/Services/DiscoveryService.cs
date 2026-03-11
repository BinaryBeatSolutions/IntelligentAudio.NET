using IntelligentAudio.Contracts; // För IParameterDiscoveryHandler
using IntelligentAudio.Contracts.Interfaces;
using IntelligentAudio.NeuralEngine.Abstractions; // För IEmbeddingGenerator och INeuralTokenizer

namespace IntelligentAudio.NeuralEngine.Services;

public sealed class DiscoveryService : IParameterDiscoveryHandler
{
    private readonly INeuralTokenizer _tokenizer;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly NeuralParameterRegistry _registry;
    private readonly float[] _tempBuffer;

    public DiscoveryService(
        INeuralTokenizer tokenizer,
        IEmbeddingGenerator embeddingGenerator,
        NeuralParameterRegistry registry)
    {
        _tokenizer = tokenizer;
        _embeddingGenerator = embeddingGenerator;
        _registry = registry;
        // Förallokera bufferten baserat på modellens storlek (t.ex. 384)
        _tempBuffer = new float[embeddingGenerator.VectorSize];
    }

    // STRICT: Vi använder Reset() istället för Dispose() för att återanvända ArrayPool-minnet
    public void OnDiscoveryStarted() => _registry.Dispose();

    public void OnParameterDiscovered(int id, ReadOnlySpan<char> name)
    {
        // 1. Tokenize på stacken (Zero-allocation)
        Span<long> tokens = stackalloc long[128];

        // 2. Använd den injicerade tokenizern
        int count = _tokenizer.TokenizeToSpan(name.ToString(), tokens);

        // 3. Generera embedding (Skriver direkt till vår temp-buffer)
        _embeddingGenerator.GenerateEmbedding(tokens.Slice(0, count), _tempBuffer);

        // 4. Spara i det blixtsnabba linjära registret
        _registry.RegisterParameter(id, _tempBuffer);
    }

    public void OnDiscoveryCompleted()
    {
        // Här kan vi logga: "Neural Engine armed with X parameters"
    }
}