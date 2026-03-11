
namespace IntelligentAudio.Contracts.Interfaces;

public interface INexusProvider
{
    /// <summary>
    /// Hämtar datan asynkront (ValueTask minimerar heap-allokering vid lokala träffar).
    /// </summary>
    ValueTask<ReadOnlyMemory<byte>> GetParameterDataAsync(ParameterKey key);

    /// <summary>
    /// Slår upp var resursen finns utan att hämta själva innehållet.
    /// </summary>
    ValueTask<NexusResource> ResolveResourceAsync(ParameterKey key);
}