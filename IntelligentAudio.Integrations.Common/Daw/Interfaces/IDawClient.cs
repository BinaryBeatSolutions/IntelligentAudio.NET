

namespace IntelligentAudio.Integrations.Common.Daw.Interfaces;

public interface IDawClient : IDisposable
{
    Guid ClientId { get; }
    string Name { get; }
    int Port { get; }

    // ValueTask är guld i .NET 10 för högpresterande I/O
    ValueTask SendChordAsync(ChordInfo chord);
    ValueTask SendCommandAsync(DawCommand command);
}

