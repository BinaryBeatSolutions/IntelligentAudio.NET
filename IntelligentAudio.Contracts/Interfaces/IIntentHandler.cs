
namespace IntelligentAudio.Contracts.Interfaces;

/// <summary>
/// Definierar en mottagare för tolkad text från AI-motorn.
/// Gör det möjligt att separera röststyrning från musikteori och DAW-kontroll.
/// </summary>
public interface IIntentHandler
{
    /// <summary>
    /// Avgör om denna handler kan hantera den identifierade texten.
    /// Exempel: "C major" -> return true (MusicTheoryHandler)
    /// </summary>
    bool CanHandle(string text);

    /// <summary>
    /// Utför den logiska handlingen kopplad till texten.
    /// </summary>
    Task HandleAsync(string text, CancellationToken ct);

    /// <summary>
    /// Anger i vilken ordning handlers ska köras om flera matchar.
    /// Lågt värde = Körs först.
    /// </summary>
    int Order => 0;
}
