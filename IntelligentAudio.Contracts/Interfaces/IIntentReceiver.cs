namespace IntelligentAudio.Contracts.Interfaces;

/// <summary>
/// An agnostic interface for any external system that wants to receive
/// interpreted data (Intents) from the AI ​​engine.
/// </summary>
public interface IIntentReceiver : IDisposable
{
    Guid Id { get; }
    string Name { get; }
    bool IsConnected { get; }

    /// <summary>
    /// Receives a generic object (e.g. ChordInfo or DawCommand).
    /// It is up to the implementation to know how to handle the data.
    /// </summary>
    ValueTask ReceiveAsync<T>(T intent, CancellationToken ct) where T : class;
}
