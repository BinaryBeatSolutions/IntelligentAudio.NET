

namespace IntelligentAudio.Contracts.Interfaces;

public interface IAudioBufferProvider
{
    float CalculateRms(ReadOnlySpan<short> samples);
    float CalculateRms(ReadOnlySpan<float> samples); // NY! För resamplad data
    void ProcessResampling(ReadOnlySpan<float> source, Span<float> destination);
}