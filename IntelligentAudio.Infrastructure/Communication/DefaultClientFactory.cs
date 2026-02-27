
using IntelligentAudio.Infrastructure.Communication;

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
            logger.LogInformation("Skapar ny {DawType}-klient (ID: {Id}) på port {Port}",
                dawType.ToUpper(), id, port);

            // Vi skapar en specifik logger för den nya klient-instansen
            var clientLogger = loggerFactory.CreateLogger<OscAbletonClient>();

            return dawType.ToLower() switch
            {
                "ableton" => new OscAbletonClient(id, "127.0.0.1", port, clientLogger),
                // "flstudio" => new MidiFlStudioClient(id, loggerFactory.CreateLogger<MidiFlStudioClient>()),
                _ => throw new NotSupportedException($"DAW-typen '{dawType}' stöds inte ännu.")
            };
        });
    }

    public IDawClient? GetClient(Guid clientId)
    {
        if (_clients.TryGetValue(clientId, out var client))
        {
            return client;
        }

        logger.LogWarning("Begärde klient {Id} som inte existerar i fabriken.", clientId);
        return null;
    }
}
