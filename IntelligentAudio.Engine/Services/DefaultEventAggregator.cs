

namespace IntelligentAudio.Engine.Services;


public class DefaultEventAggregator : IEventAggregator
{
    // We use a thread-safe dictionary to hold one channel per event type
    private readonly ConcurrentDictionary<Type, object> _channels = new();

    public void Publish<T>(T @event) where T : class
    {
        var channel = GetOrCreateChannel<T>();

        // TryWrite is extremely fast and non-blocking
        if (!channel.Writer.TryWrite(@event))
        {
            // Detta händer bara om kanalen är full (vilket vi inte satt i Unbounded)
        }
    }

    /// <summary>
    /// Subcribe
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ct"></param>
    /// <returns></returns>
    public IAsyncEnumerable<T> Subscribe<T>(CancellationToken ct) where T : class
    {
        var channel = GetOrCreateChannel<T>();

        //ReadAllAsync is gold in .NET 10 for 'await foreach'
        return channel.Reader.ReadAllAsync(ct);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
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