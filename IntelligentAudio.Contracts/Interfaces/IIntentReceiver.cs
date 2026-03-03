namespace IntelligentAudio.Contracts.Interfaces;

/// <summary>
/// Ett agnostiskt gränssnitt för alla externa system som vill ta emot 
/// tolkad data (Intents) från AI-motorn.
/// </summary>
public interface IIntentReceiver : IDisposable
{
    Guid Id { get; }
    string Name { get; }
    bool IsConnected { get; }

    /// <summary>
    /// Tar emot ett generiskt objekt (t.ex. ChordInfo eller DawCommand).
    /// Det är upp till implementationen att veta hur datan ska hanteras.
    /// </summary>
    Task ReceiveAsync<T>(T intent, CancellationToken ct) where T : class;
}
