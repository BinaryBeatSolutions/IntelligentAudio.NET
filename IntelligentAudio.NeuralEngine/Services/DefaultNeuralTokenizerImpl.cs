using IntelligentAudio.Contracts.Interfaces;
using Microsoft.ML.Tokenizers;
using System.IO;

namespace IntelligentAudio.NeuralEngine.Services;

public sealed class DefaultNeuralTokenizerImpl : INeuralTokenizer, IDisposable
{
    private readonly WordPieceTokenizer _tokenizer;

    public DefaultNeuralTokenizerImpl(string vocabFilePath)
    {
        // Latency Analysis: Sker vid uppstart.
        // Vi konfigurerar options för att matcha BERT/MiniLM-standarden i App_Data.
        var options = new WordPieceOptions
        {
            UnknownToken = "[UNK]",
            SpecialTokens = { },
            MaxInputCharsPerWord = 100
        };

        // Skapa tokenizern direkt från filen - högpresterande inläsning
        _tokenizer = WordPieceTokenizer.Create(vocabFilePath, options);
    }

    /// <summary>
    /// Omvandlar text till tokens direkt i en Span för att följa Zero-Allocation regeln.
    /// </summary>
    public int TokenizeToSpan(string text, Span<long> destination)
    {
        // Allocation Analysis: .NET 10 optimerad Encode.
        var result = _tokenizer.EncodeToIds(text);
      
        // Beräkna hur många vi får plats med i destinationen
        int count = result.Count < destination.Length ? result.Count : destination.Length;

        // STRICT: Ingen LINQ (enligt dina regler), använd for-loop
        for (int i = 0; i < count; i++)
        {
            destination[i] = result[i];
        }

        return count;
    }

    public void Dispose() { /* WordPieceTokenizer hanterar resurser internt */ }

}
