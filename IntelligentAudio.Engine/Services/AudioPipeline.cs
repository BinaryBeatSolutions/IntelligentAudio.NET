

namespace IntelligentAudio.Engine.Services;

public class AudioPipeline
{
    // Vi skapar en kanal som hanterar arrayer av flyttal (ljudet)
    // 'Unbounded' betyder att den kan växa, men i realtid vill vi 
    // oftast ha 'Bounded' för att inte äta upp allt minne om AI:n laggar.
    private readonly Channel<float[]> _channel = Channel.CreateBounded<float[]>(new BoundedChannelOptions(100)
    {
        FullMode = BoundedChannelFullMode.DropOldest // Viktigt för realtid: Släng gammalt ljud om vi inte hinner med!
    });

    public ChannelWriter<float[]> Writer => _channel.Writer;
    public ChannelReader<float[]> Reader => _channel.Reader;
}