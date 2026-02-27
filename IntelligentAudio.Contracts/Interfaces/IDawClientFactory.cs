

namespace IntelligentAudio.Contracts.Interfaces;

public interface IDawClientFactory
{
    IDawClient? GetClient(Guid clientId);
    // Skapar en klient baserat på typ (t.ex. från en config eller handshake)
    IDawClient CreateClient(Guid clientId, int port, string dawType = "Ableton");
}