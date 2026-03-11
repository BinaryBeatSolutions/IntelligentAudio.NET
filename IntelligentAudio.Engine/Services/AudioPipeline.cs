

namespace IntelligentAudio.Engine.Services;

public class AudioPipeline
{
    private readonly Channel<float[]> _channel = Channel.CreateBounded<float[]>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.DropOldest
    });

    public ChannelWriter<float[]> Writer => _channel.Writer;
    public ChannelReader<float[]> Reader => _channel.Reader;
}