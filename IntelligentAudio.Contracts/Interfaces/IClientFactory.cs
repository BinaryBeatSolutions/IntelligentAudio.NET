
namespace IntelligentAudio.Contracts.Interfaces;

public interface IClientFactory
{
    // Hämtar en existerande klient eller skapar en ny baserat på ID/Port
    IDawClient? GetClient(Guid clientId);

    // Registrerar en ny klient (t.ex. när ett Handshake-OSC kommer in)
    IDawClient CreateClient(Guid clientId, int port);
}