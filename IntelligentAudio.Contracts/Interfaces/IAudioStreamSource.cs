

namespace IntelligentAudio.Contracts;

public interface IAudioStreamSource
{
    // A thread-safe, non-blocking stream of 16kHz resampled audio buffers
    ChannelReader<float[]> AudioStream { get; }

    // Status for the UI/AMXD
    bool IsRecording { get; }
}