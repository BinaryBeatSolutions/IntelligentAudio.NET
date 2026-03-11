
namespace IntelligentAudio.Infrastructure.Controllers;

public sealed partial class DefaultDawCommandControllerImpl(
    IDawClientFactory clientFactory,
    ILogger<DefaultDawCommandControllerImpl> logger) : IDawCommandController
{
    public async ValueTask ExecuteAsync(Guid clientId, DawCommand command)
    {
        // Lookup (Zero-Alloc)
        var client = clientFactory.GetClient(clientId);

        if (client is null)
        {
            LogClientNotFound(logger, clientId);
            return;
        }

        // Dispatch (Skickar vidare structen direkt)
        await client.SendCommandAsync(command);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Client {ClientId} not found.")]
    static partial void LogClientNotFound(ILogger logger, Guid clientId);
}