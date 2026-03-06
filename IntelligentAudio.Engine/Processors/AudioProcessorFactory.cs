
namespace IntelligentAudio.Engine.Processors;

public class AudioProcessorFactory : IAudioProcessorFactory
{
    public IAudioProcessor CreateHighPassFilter(FilterType type, float cutoff, int sampleRate)
    {
        // Här mappar vi ENUM till KONKRET KLASS.
        // Detta är den enda platsen som behöver känna till alla filter-typer.
        return type switch
        {
            FilterType.Simple => new SimpleHighPassFilter(cutoff, sampleRate),
            FilterType.Butterworth12dB => new ButterworthHighPassFilter12dB(cutoff, sampleRate),
            FilterType.Butterworth24dB => new ButterworthHighPassFilter24dB(cutoff, sampleRate),
            _ => new SimpleHighPassFilter(cutoff, sampleRate)
        };
    }
}