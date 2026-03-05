
using IntelligentAudio.Contracts.Models;

namespace IntelligentAudio.Integrations.Common.Daw.Interfaces;

public interface IDawClientProvider
{
    // Använd string för matchning, men överväg en enum i Contracts senare
    bool CanHandle(DawType dawType);

    // Vi skickar med det som behövs för att bygga klienten
    IDawClient CreateInstance(Guid clientId, int port);
}