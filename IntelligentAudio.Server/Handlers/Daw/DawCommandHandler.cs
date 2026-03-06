
namespace IntelligentAudio.Server.Handlers.Daw;

public sealed partial class DawCommandHandler(
    IDawCommandController dawController,
    IClientManager clientManager,
    ILogger<DawCommandHandler> logger) : IIntentHandler
{
    // Vi använder en struct-baserad array för att undvika Dictionary-overhead i sökningen
    private static readonly (string Key, DawAction Action)[] CommandMap =
    [
        ("play", DawAction.Play),
        ("stop", DawAction.Stop),
        ("record", DawAction.Record),
        ("loop", DawAction.SetLoop) // Justera efter din enum
    ];

    public bool CanHandle(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        // Manuell loop = ZERO allocation (ingen iterator)
        for (int i = 0; i < CommandMap.Length; i++)
        {
            if (text.Contains(CommandMap[i].Key, StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    public async Task HandleAsync(string text, CancellationToken ct)
    {
        // 1. Hämta GUID för den klient som är "fokuserad" eller senast aktiv
        var activeClientId = clientManager.GetActiveClientId();

        if (activeClientId == Guid.Empty)
        {
            LogNoActiveClient(logger);
            return;
        }

        // 2. Matcha och exekvera
        for (int i = 0; i < CommandMap.Length; i++) 
        {
            var (key, action) = CommandMap[i];
            if (text.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                LogIntentMatched(logger, text, action);

                // Skapa kommandot (struct)
                var command = new DawCommand(action);

                // Exekvera via den nya kontrollern
                await dawController.ExecuteAsync(activeClientId, command);
                return;
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "[DAW] Intent matched: '{Text}' -> Action: {Action}")]
    static partial void LogIntentMatched(ILogger logger, string text, DawAction action);

    [LoggerMessage(Level = LogLevel.Warning, Message = "[DAW] Command ignored: No active DAW client registered.")]
    static partial void LogNoActiveClient(ILogger logger);
}
