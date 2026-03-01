

namespace IntelligentAudio.Infrastructure.Audio;

public class MacAudioSource : IAudioStreamSource, IDisposable
{
    public ChannelReader<float[]> AudioStream => throw new NotImplementedException();

    public bool IsRecording => throw new NotImplementedException();

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}

