
namespace IntelligentAudio.Integrations.Common.Daw.Interfaces;

public interface IDawCommandController
{
    /// <summary>
    /// Dirigerar ett kommando till rätt klient. 
    /// Använder ValueTask för Zero-Allocation i anropskedjan.
    /// </summary>
    ValueTask ExecuteAsync(Guid clientId, DawCommand command);
}