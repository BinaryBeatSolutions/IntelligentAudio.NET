
namespace IntelligentAudio.NeuralEngine.Services;

public sealed class NeuralBackgroundWorker : BackgroundService
{
    private readonly ChannelReader<string> _inputReader;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly DefaultNeuralTokenizerImpl _tokenizer;
    // Förallokerad buffer för embedding (t.ex. 384 floats)
    private readonly float[] _embeddingBuffer;

    public NeuralBackgroundWorker(
        ChannelReader<string> inputReader,
        IEmbeddingGenerator embeddingGenerator,
        DefaultNeuralTokenizerImpl tokenizer)
    {
        _inputReader = inputReader;
        _embeddingGenerator = embeddingGenerator;
        _tokenizer = tokenizer;
        _embeddingBuffer = new float[embeddingGenerator.VectorSize];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // STRICT: Vi använder 'while' och 'WaitToReadAsync' för zero-alloc konsumtion
        while (await _inputReader.WaitToReadAsync(stoppingToken))
        {
            while (_inputReader.TryRead(out var voiceText))
            {
                ProcessIntent(voiceText);
            }
        }
    }

    private void ProcessIntent(string text)
    {
        // 1. Tokenize (STRICT: stackalloc för korta röstkommandon < 128 tokens)
        Span<long> tokens = stackalloc long[128];
        int tokenCount = _tokenizer.TokenizeToSpan(text, tokens);

        // 2. Generate Embedding (Skriver direkt till vår förallokerade buffer)
        _embeddingGenerator.GenerateEmbedding(tokens.Slice(0, tokenCount), _embeddingBuffer);

        // 3. Semantic Match (Här anropar vi din SIMD-matematik)
        // Vi jämför _embeddingBuffer mot din förberäknade parameter-cache
        ResolveBestParameterMatch(_embeddingBuffer);
    }

    private void ResolveBestParameterMatch(ReadOnlySpan<float> voiceVector)
    {
        // TODO: Här loopar vi igenom din Dictionary<int, float[]> från Ableton
        // och använder VectorOperations.CosineSimilarity.
        // Resultatet skickas till din IIntentHandler.
    }
}