

namespace IntelligentAudio.Engine.Services;

public class AudioPipeline
{
    // We create a channel that handles arrays of floating point numbers (the sound)
    // 'Unbounded' means it can grow, but in real time we
    // usually want 'Bounded' to not eat up all the memory if the AI ​​lags.

    private readonly Channel<AudioSegment> _channel = Channel.CreateBounded<AudioSegment>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.DropOldest // Important for real time: Throw away old audio if we don't have time
    });

    public ChannelWriter<AudioSegment> Writer => _channel.Writer;
    public ChannelReader<AudioSegment> Reader => _channel.Reader;
}


