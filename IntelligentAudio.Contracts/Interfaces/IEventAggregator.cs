
namespace IntelligentAudio.Contracts.Interfaces;

public interface IEventAggregator
{
    // Publicera ett event (t.ex. ChordDetectedEvent)
    void Publish<T>(T @event) where T : class;

    // Prenumerera på ett event asynkront (perfekt för .NET 10 await foreach)
    IAsyncEnumerable<T> Subscribe<T>(CancellationToken ct) where T : class;
}