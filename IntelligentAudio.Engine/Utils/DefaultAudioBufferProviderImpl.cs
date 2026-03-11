

namespace IntelligentAudio.Engine.Utils;


// I DefaultAudioBufferProviderImpl.cs (Engine)
public partial class DefaultAudioBufferProviderImpl : IAudioBufferProvider
{
    // Befintlig short-version (för rådata/NoiseGate)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float CalculateRms(ReadOnlySpan<short> audioData)
        => AudioMath.CalculateRms(audioData);

    // NY: Float-version (för resamplad data/Whisper-input)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float CalculateRms(ReadOnlySpan<float> audioData)
        => AudioMath.CalculateRms(audioData);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ProcessResampling(ReadOnlySpan<float> source, Span<float> destination)
        => AudioMath.Resample(source, destination);
}