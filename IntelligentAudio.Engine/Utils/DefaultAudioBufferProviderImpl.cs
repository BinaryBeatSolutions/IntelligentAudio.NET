

namespace IntelligentAudio.Engine.Utils;


public sealed class DefaultAudioBufferProviderImpl : IAudioBufferProvider
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public float GetRms(ReadOnlySpan<short> audioData)
        => AudioMath.CalculateRms(audioData);

    public void ProcessResampling(ReadOnlySpan<float> source, Span<float> destination)
        => AudioMath.Resample(source, destination);
};

