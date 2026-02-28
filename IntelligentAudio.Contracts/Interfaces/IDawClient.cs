

namespace IntelligentAudio.Contracts.Interfaces;

public interface IDawClient : IDisposable
{
    Guid ClientId { get; }
    string Name { get; }
    int Port => 0;

    Task SendChordAsync(ChordInfo chord);
    Task SendCommandAsync(DawCommand command);
}