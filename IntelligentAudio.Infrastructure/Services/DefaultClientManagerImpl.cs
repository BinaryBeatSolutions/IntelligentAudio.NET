namespace IntelligentAudio.Infrastructure.Services;

using IntelligentAudio.Contracts.Interfaces;

public sealed class DefaultClientManagerImpl : IClientManager
{
    private Guid _activeClientId = Guid.Empty;

    public void SetActiveClient(Guid clientId) => _activeClientId = clientId;
    public Guid GetActiveClientId() => _activeClientId;
    public void RemoveClient(Guid clientId)
    {
        if (_activeClientId == clientId) _activeClientId = Guid.Empty;
    }
}