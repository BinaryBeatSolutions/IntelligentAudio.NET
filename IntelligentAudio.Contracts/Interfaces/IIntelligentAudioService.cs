
namespace IntelligentAudio.Contracts.Interfaces;

public interface IIntelligentAudioService
{
    Task ProcessAudioAsync(float[] samples, int length, CancellationToken ct);
    Task EnsureModelReadyAsync(string model, CancellationToken ct);

    ValueTask ProcessAudioAsync(ReadOnlyMemory<float> samples, CancellationToken ct);

}