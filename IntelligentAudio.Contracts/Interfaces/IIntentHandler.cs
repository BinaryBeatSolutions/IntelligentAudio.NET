
namespace IntelligentAudio.Contracts.Interfaces;

/// <summary>
/// Defines a receiver for the recognized text from the AI ​​engine.
/// Allows for the separation of voice control from music theory and DAW control or other apps.
/// </summary>
public interface IIntentHandler
{
    /// <summary>
    /// Determines whether this handler can handle the recognized text.
    /// Example: "C major" -> return true (MusicTheoryHandler)
    /// </summary>
    bool CanHandle(string text);

    /// <summary>
    /// Performs the logical action associated with the text.
    /// </summary>
    Task HandleAsync(string text, CancellationToken ct);

    /// <summary>
    /// Specifies the order in which handlers should be executed if multiple matches are found.
    /// Low value = Execute first.
    /// </summary>
    int Order => 0;
}