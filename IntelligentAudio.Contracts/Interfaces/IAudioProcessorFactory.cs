


namespace IntelligentAudio.Contracts.Interfaces;

public interface IAudioProcessorFactory
{
    // Fabriken tar in de primitiva värdena direkt. 
    // Inga konfigurations-objekt här!
    IAudioProcessor CreateHighPassFilter(FilterType type, float cutoff, int sampleRate);
}