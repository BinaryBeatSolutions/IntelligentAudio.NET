
namespace IntelligentAudio.Integrations.Common.Daw.Interfaces;

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