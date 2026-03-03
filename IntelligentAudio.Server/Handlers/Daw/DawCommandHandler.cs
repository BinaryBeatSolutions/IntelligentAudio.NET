



namespace IntelligentAudio.Server.Handlers.Daw;

public class DawCommandHandler(IDawController dawController, ILogger<DawCommandHandler> logger) : IIntentHandler
{
    private static readonly Dictionary<string, DawAction> CommandMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "play", DawAction.Play },
        { "stop", DawAction.Stop },
        { "record", DawAction.Record },
        { "loop", DawAction.ToggleLoop }
    };

    public bool CanHandle(string text) =>
        !string.IsNullOrWhiteSpace(text) &&
        CommandMap.Keys.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

    public async Task HandleAsync(string text, CancellationToken ct)
    {
        // Hitta vilket kommando som matchar i texten
        var key = CommandMap.Keys.FirstOrDefault(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

        if (key != null)
        {
            var action = CommandMap[key];
            logger.LogInformation("[DAW] Intent matched: '{text}' -> Action: {action}", text, action);

            // Vi skapar ett generiskt kommando och skickar det till kontrollern
            var command = new DawCommand(action);
            await dawController.ExecuteAsync(command, ct);
        }
    }
}