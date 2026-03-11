

namespace IntelligentAudio.Contracts;

public interface INeuralModelService
{
    string ModelPath { get; }
    string VocabPath { get; }

    /// <summary>
    /// Verifierar att AI-modellerna finns lokalt. Laddar ner vid behov.
    /// </summary>
    ValueTask EnsureModelReadyAsync(CancellationToken ct);
}