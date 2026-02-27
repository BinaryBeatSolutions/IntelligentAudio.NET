

namespace IntelligentAudio.Engine.Services;

public class DefaultEventAggregator : IEventAggregator
{
    private readonly ConcurrentDictionary<Type, object> _channels = new();

    public void Publish<T>(T @event) where T : class
    {
        var channel = GetOrCreateChannel<T>();
        channel.Writer.TryWrite(@event);
    }

    public IAsyncEnumerable<T> Subscribe<T>(CancellationToken ct) where T : class
    {
        var channel = GetOrCreateChannel<T>();
        return channel.Reader.ReadAllAsync(ct);
    }

    private Channel<T> GetOrCreateChannel<T>() where T : class
    {
        // Vi skapar en kanal per Event-typ (Chord, Transport, etc.)
        return (Channel<T>)_channels.GetOrAdd(typeof(T), _ => Channel.CreateUnbounded<T>());
    }
}

