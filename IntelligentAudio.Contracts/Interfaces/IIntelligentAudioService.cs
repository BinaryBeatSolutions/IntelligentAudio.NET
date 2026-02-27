
namespace IntelligentAudio.Contracts.Interfaces;

public interface IIntelligentAudioService
{
    Task ProcessAudioAsync(float[] samples, CancellationToken ct);
}