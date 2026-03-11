
namespace IntelligentAudio.Contracts.Interfaces;

public interface INeuralTokenizer : IDisposable
{
    /// <summary>
    /// Omvandlar text till tokens direkt i en Span för att undvika heap-allokeringar.
    /// </summary>
    /// <param name="text">Råtexten från Whisper</param>
    /// <param name="destination">En förallokerad buffer (ofta stackalloc)</param>
    /// <returns>Antalet tokens som faktiskt skrevs till destinationen</returns>
    int TokenizeToSpan(string text, Span<long> destination);
}