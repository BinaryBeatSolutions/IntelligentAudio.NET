
namespace IntelligentAudio.Contracts.Interfaces;

public interface IHandshakeListener : IDisposable
{
    // Startar OSC-servern på port 9005
    ValueTask StartListeningAsync(CancellationToken ct);
}