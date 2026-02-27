

namespace IntelligentAudio.Engine.Services;


public class DefaultEventAggregator : IEventAggregator
{
    // Vi använder en trådsäker dictionary för att hålla en kanal per event-typ
    private readonly ConcurrentDictionary<Type, object> _channels = new();

    public void Publish<T>(T @event) where T : class
    {
        var channel = GetOrCreateChannel<T>();

        // TryWrite är extremt snabb och icke-blockerande
        if (!channel.Writer.TryWrite(@event))
        {
            // Detta händer bara om kanalen är full (vilket vi inte satt i Unbounded)
        }
    }

    public IAsyncEnumerable<T> Subscribe<T>(CancellationToken ct) where T : class
    {
        var channel = GetOrCreateChannel<T>();

        // ReadAllAsync är guld i .NET 10 för 'await foreach'
        return channel.Reader.ReadAllAsync(ct);
    }

    private Channel<T> GetOrCreateChannel<T>() where T : class
    {
        return (Channel<T>)_channels.GetOrAdd(typeof(T), _ =>
            Channel.CreateUnbounded<T>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            }));
    }
}