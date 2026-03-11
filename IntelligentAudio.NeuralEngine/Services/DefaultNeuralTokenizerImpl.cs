
using Microsoft.ML.Tokenizers;

namespace IntelligentAudio.NeuralEngine.Services;

public sealed class DefaultNeuralTokenizerImpl : INeuralTokenizer, IDisposable
{
    private WordPieceTokenizer? _tokenizer;
    private string _vocabFilePath;

    public DefaultNeuralTokenizerImpl(string vocabFilePath)
    {
        _vocabFilePath = vocabFilePath;
    }

    private void EnsureInitialized()
    {
        // Latency Analysis: Sker endast första gången efter download.
        if (_tokenizer != null) return;

        if (!File.Exists(_vocabFilePath))
            throw new FileNotFoundException("Vocab file missing. EnsureModelReadyAsync must run first.");

        var options = new WordPieceOptions
        {
            UnknownToken = "[UNK]",
            SpecialTokens = { },
            MaxInputCharsPerWord = 100
        };

        _tokenizer = WordPieceTokenizer.Create(_vocabFilePath, new WordPieceOptions() { });
    }

    /// <summary>
    /// Omvandlar text till tokens direkt i en Span för att följa Zero-Allocation regeln.
    /// </summary>
    public int TokenizeToSpan(string text, Span<long> destination)
    {
        // STRICT: Om _tokenizer är null, skapa den nu (eftersom filen finns på disk)
        if (_tokenizer == null)
        {
            var options = new WordPieceOptions {  };
            // _vocabPath skickades in i konstruktorn
            _tokenizer = WordPieceTokenizer.Create(_vocabFilePath, options);
        }

        // Nu kan vi säkert anropa EncodeToIds
        var result = _tokenizer.EncodeToIds(text.ToString());

        int count = result.Count < destination.Length ? result.Count : destination.Length;
        for (int i = 0; i < count; i++)
        {
            destination[i] = result[i];
        }

        return count;
    }

    public void Dispose() { /* WordPieceTokenizer hanterar resurser internt */ }

}
