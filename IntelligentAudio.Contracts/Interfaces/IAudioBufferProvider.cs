

namespace IntelligentAudio.Contracts.Interfaces;

public interface IAudioBufferProvider
{
    void ProcessResampling(ReadOnlySpan<float> source, Span<float> destination);
    float GetRms(ReadOnlySpan<short> samples);
}