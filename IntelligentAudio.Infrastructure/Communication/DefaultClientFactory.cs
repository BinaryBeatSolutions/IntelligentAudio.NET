

namespace IntelligentAudio.Infrastructure.Communication;

public class DefaultClientFactory(ILogger<DefaultClientFactory> logger) : IClientFactory
{
    private readonly ConcurrentDictionary<Guid, IAbletonClient> _clients = new();

    public IAbletonClient? GetClient(Guid clientId)
        => _clients.TryGetValue(clientId, out var client) ? client : null;

    public IDawClient CreateClient(Guid clientId, int port)
    {
        return _clients.GetOrAdd(clientId, id =>
        {
            logger.LogInformation("Skapar ny Ableton-klient på port {Port}", port);
            return new OscAbletonClient(id, "127.0.0.1", port);
        });
    }
}