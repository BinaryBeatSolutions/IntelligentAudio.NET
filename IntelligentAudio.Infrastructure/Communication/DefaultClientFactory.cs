
using IntelligentAudio.Contracts.Models;

namespace IntelligentAudio.Infrastructure.Factories;


public sealed class DefaultDawClientFactory : IDawClientFactory
{
    private readonly ConcurrentDictionary<Guid, IDawClient> _clients = new();
    private readonly IDawClientProvider[] _providers; // Array för snabb iteration utan alloc

    public DefaultDawClientFactory(IEnumerable<IDawClientProvider> providers)
    {
        _providers = providers.ToArray(); // Görs bara en gång vid start
    }

    public IDawClient CreateClient(Guid clientId, int port, DawType dawType)
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

            return null;
        }, (this, port, dawType)); // State-tuple
    }

    public IDawClient? GetClient(Guid clientId)
    {
        return _clients.TryGetValue(clientId, out var client) ? client : null;
    }
}
