

using IntelligentAudio.Integrations.Common.Daw;

/// <summary>
/// Ett agnostiskt gränssnitt för att styra en DAW.
/// Implementeras av specifika integrations-plugins (t.ex. Ableton, Logic).
/// </summary>
public interface IDawController
{
    /// <summary>
    /// Utför ett generiskt DAW-kommando (Play, Stop, etc.)
    /// </summary>
    Task ExecuteAsync(DawCommand command, CancellationToken ct);
}



//public class DawCommandHandler(ILogger<DawCommandHandler> logger) : IIntentHandler
//{
//    private static readonly Dictionary<string, DawAction> CommandMap = new(StringComparer.OrdinalIgnoreCase)
//    {
//        { "play", DawAction.Play },
//        { "stop", DawAction.Stop },
//        { "record", DawAction.Record },
//        { "loop", DawAction.ToggleLoop }
//    };

//    public bool CanHandle(string text) =>
//        !string.IsNullOrWhiteSpace(text) &&
//        CommandMap.Keys.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));

//    public async Task HandleAsync(string text, CancellationToken ct)
//    {
//        var key = CommandMap.Keys.FirstOrDefault(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
//        if (key != null)
//        {
//            var action = CommandMap[key];
//            // Vi skapar ett generiskt kommando
//            var command = new DawCommand(action);

//            // Vi skickar det till kontrollern - vi vet INTE att det är OSC!
//            await dawController.ExecuteAsync(command, ct);
//        }
//    }
//}

