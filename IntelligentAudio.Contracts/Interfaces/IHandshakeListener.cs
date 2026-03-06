
namespace IntelligentAudio.Contracts.Interfaces;

public interface IHandshakeListener : IDisposable
{
    // Starts OSC-servern on port 9005
    ValueTask StartListeningAsync(CancellationToken ct);
}