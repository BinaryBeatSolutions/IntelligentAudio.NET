
namespace IntelligentAudio.Contracts.Interfaces;

public interface IClientManager
{
    // Håller koll på vilken klient som Whisper-intents ska skickas till
    void SetActiveClient(Guid clientId);
    Guid GetActiveClientId();
    void RemoveClient(Guid clientId);
}