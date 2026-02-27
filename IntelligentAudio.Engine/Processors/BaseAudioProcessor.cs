


namespace IntelligentAudio.Engine.Processors;

public abstract class BaseAudioProcessor : IAudioProcessor
{
    // Varje processor får ett namn för loggning/UI
    public abstract string Name { get; }

    // Möjlighet att stänga av/på filtret i realtid
    public bool IsEnabled { get; set; } = true;

    // Denna metod anropas av motorn
    public void Process(Span<float> buffer)
    {
        if (!IsEnabled || buffer.IsEmpty) return;

        // Här körs den faktiska algoritmen som implementeras i underklasserna
        OnProcess(buffer);
    }

    protected abstract void OnProcess(Span<float> buffer);
}