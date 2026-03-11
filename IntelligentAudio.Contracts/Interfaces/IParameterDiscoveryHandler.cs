
namespace IntelligentAudio.Contracts.Interfaces;

public interface IParameterDiscoveryHandler
{
    // Vi skickar in ID och Namn direkt. 
    // Ingen referens till OscCore här.
    void OnParameterDiscovered(int id, ReadOnlySpan<char> name);

    // För att veta när en ny plugin-lista börjar/slutar
    void OnDiscoveryStarted();
    void OnDiscoveryCompleted();
}