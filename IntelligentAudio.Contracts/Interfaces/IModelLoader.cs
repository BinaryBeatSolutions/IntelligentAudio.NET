

namespace IntelligentAudio.Contracts.Interfaces;

public interface IModelLoader
{
    Task<AIModel> LoadAsync(CancellationToken ct);
}

