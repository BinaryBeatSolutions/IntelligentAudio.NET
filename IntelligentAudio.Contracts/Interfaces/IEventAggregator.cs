
namespace IntelligentAudio.Contracts.Interfaces;

public interface IEventAggregator
{
    // Publicera ett event (t.ex. ChordDetectedEvent)
    void Publish<T>(T @event) where T : class;

    // Subscribe to event asyncronus (perfect for .NET 10 await foreach)
    IAsyncEnumerable<T> Subscribe<T>(CancellationToken ct) where T : class;
}