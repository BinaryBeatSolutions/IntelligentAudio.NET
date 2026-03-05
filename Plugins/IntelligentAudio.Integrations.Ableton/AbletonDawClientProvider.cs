using IntelligentAudio.Contracts.Models;

namespace IntelligentAudio.Integrations.Ableton;

public sealed class AbletonDawClientProvider(ILoggerFactory loggerFactory) : IDawClientProvider
{

    public bool CanHandle(DawType dawType) => dawType == DawType.Ableton;

    public IDawClient CreateInstance(Guid clientId, int port)
    {
        var logger = loggerFactory.CreateLogger<AbletonDawClient>();
        return new AbletonDawClient(clientId, port, logger);
    }
}