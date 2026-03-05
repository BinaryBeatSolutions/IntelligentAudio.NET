
namespace IntelligentAudio.Infrastructure.Factories;


public sealed class DefaultDawClientFactory : IDawClientFactory
{
    private readonly ConcurrentDictionary<Guid, IDawClient> _clients = new();
    private readonly IDawClientProvider[] _providers; // Array för snabb iteration utan alloc

    public DefaultDawClientFactory(IEnumerable<IDawClientProvider> providers)
    {
        _providers = providers.ToArray(); // Görs bara en gång vid start
    }

    public IDawClient CreateClient(Guid clientId, int port, string dawType)
    {
        // Vi skickar in (this, port, dawType) som state
        return _clients.GetOrAdd(clientId, static (id, state) =>
        {
            // Packa upp state utan att allokera
            var (factory, p, type) = state;

            // Använd for-loop för att undvika IEnumerable-enumerator (allokering)
            var providers = factory._providers;
            for (int i = 0; i < providers.Length; i++)
            {
                if (providers[i].CanHandle(type))
                {
                    return providers[i].CreateInstance(id, p);
                }
            }

            throw new NotSupportedException($"DAW {type} not supported.");
        }, (this, port, dawType)); // State-tuple
    }

    public IDawClient? GetClient(Guid clientId)
    {
        // TryGetValue på en Guid-nyckel är O(1) och genererar noll heap-skräp.
        // Vi returnerar null om klienten inte finns (t.ex. om den kopplat ifrån).
        return _clients.TryGetValue(clientId, out var client) ? client : null;
    }
}
