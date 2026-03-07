
namespace IntelligentAudio.NeuralEngine.Services;

public sealed class SemanticIntentInterpreter
{
    // Vi använder ett fast schema för att undvika allokeringar vid varje anrop
    private static readonly Dictionary<string, float> _absoluteCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        { "open", 1.0f }, { "max", 1.0f }, { "full", 1.0f }, { "bypass", 1.0f },
        { "close", 0.0f }, { "zero", 0.0f }, { "off", 0.0f }, { "mute", 0.0f },
        { "half", 0.5f }, { "mid", 0.5f }, { "center", 0.5f }
    };

    // Relativa värden (Deltas)
    private static readonly Dictionary<string, float> _relativeCommands = new(StringComparer.OrdinalIgnoreCase)
    {
        { "increase", 0.1f }, { "raise", 0.1f }, { "more", 0.1f }, { "up", 0.1f },
        { "decrease", -0.1f }, { "lower", -0.1f }, { "less", -0.1f }, { "down", -0.1f },
        { "bit", 0.05f }, { "little", 0.05f } // Kan användas för finjustering
    };

    public bool TryResolveValue(ReadOnlySpan<char> voiceText, out float value, out bool isRelative)
    {
        // Latency Analysis: Enkel sträng-lookup är O(1).
        // Vi letar efter nyckelord i den röst-text vi fick från Whisper.

        // (Här kan vi köra en enkel loop över orden i voiceText)
        // För detta exempel kör vi en förenklad version:
        string text = voiceText.ToString();

        foreach (var cmd in _absoluteCommands)
        {
            if (text.Contains(cmd.Key))
            {
                value = cmd.Value;
                isRelative = false;
                return true;
            }
        }

        foreach (var cmd in _relativeCommands)
        {
            if (text.Contains(cmd.Key))
            {
                value = cmd.Value;
                isRelative = true;
                return true;
            }
        }

        value = 0;
        isRelative = false;
        return false;
    }
}