
namespace IntelligentAudio.Contracts.Interfaces;

/// <summary>
/// Process pipeline AUDIO
/// </summary>
public interface IIntelligentAudioService
{
    Task ProcessAudioAsync(float[] samples, int length, CancellationToken ct);

    Task EnsureModelReadyAsync(string model, CancellationToken ct);

    ValueTask ProcessAudioAsync(ReadOnlyMemory<float> samples, CancellationToken ct);
}