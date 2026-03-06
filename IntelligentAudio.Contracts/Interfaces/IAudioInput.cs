
namespace IntelligentAudio.Contracts.Interfaces;

public interface IAudioInput
{
    // Returnerar t.ex. 44100, 48000 eller 96000 beroende på hårdvara
    int SampleRate { get; }

    // Event eller callback när ny data finns
    event Action<Span<float>> DataAvailable;
}