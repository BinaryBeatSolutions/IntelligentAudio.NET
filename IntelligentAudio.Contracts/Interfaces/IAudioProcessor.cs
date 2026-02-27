
namespace IntelligentAudio.Contracts.Interfaces;

public interface IAudioProcessor
{
    void Process(Span<float> buffer);
    bool IsEnabled { get; set; }
}