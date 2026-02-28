
namespace IntelligentAudio.Infrastructure.Factories;

public class DefaultDawClientFactory(
    ILoggerFactory loggerFactory,
    ILogger<DefaultDawClientFactory> logger) : IDawClientFactory
{
    private readonly ConcurrentDictionary<Guid, IDawClient> _clients = new();

    public IDawClient CreateClient(Guid clientId, int port, string dawType)
    {
        return _clients.GetOrAdd(clientId, id =>
        {
            logger.LogInformation("Creates new {DawType}-client (ID: {Id}) on port {Port}",
                dawType.ToUpper(), id, port);

            // Vi skapar en specifik logger för den nya klient-instansen
            var clientLogger = loggerFactory.CreateLogger<OscAbletonClient>();

            return dawType.ToLower() switch
            {
                "ableton" => new OscAbletonClient(id, "127.0.0.1", port, clientLogger),
                // "flstudio" => new MidiFlStudioClient(id, loggerFactory.CreateLogger<MidiFlStudioClient>()),
                _ => throw new NotSupportedException($"DAW-typ '{dawType}' not supported yet.")
            };
        });
    }

    public IDawClient? GetClient(Guid clientId)
    {
        if (_clients.TryGetValue(clientId, out var client))
        {
            return client;
        }

        logger.LogWarning("Client id: {Id} dont exist.", clientId);
        return null;
    }
}