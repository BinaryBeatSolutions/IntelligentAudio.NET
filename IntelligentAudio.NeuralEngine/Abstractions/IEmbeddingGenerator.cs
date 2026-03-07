
namespace IntelligentAudio.NeuralEngine.Abstractions;

public interface IEmbeddingGenerator
{
    // Vi skickar in en destination Span för att undvika att metoden 
    // allokerar och returnerar en ny array på heapen.
    void GenerateEmbedding(ReadOnlySpan<long> tokens, Span<float> destination);
    int VectorSize { get; }
}