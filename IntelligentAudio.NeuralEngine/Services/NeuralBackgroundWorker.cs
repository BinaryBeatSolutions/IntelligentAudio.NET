
using Microsoft.Extensions.Logging;
using Microsoft.ML.Tokenizers;

namespace IntelligentAudio.NeuralEngine.Services;

public sealed class NeuralBackgroundWorker : BackgroundService
{
    private readonly ILogger _logger;   
    private readonly ChannelReader<string> _intentReader;
    private readonly INeuralTokenizer _tokenizer;
    private readonly IEmbeddingGenerator _embeddingGenerator;
    private readonly NeuralParameterRegistry _registry;
    private readonly NeuralOscActionHandler _actionHandler;
    private readonly INeuralModelService _modelService;
    private readonly DefaultSemanticIntentInterpreterImpl _intentInterpreter;

    // Förallokerad buffer för röst-embedding (t.ex. 384 floats)
    private readonly float[] _voiceEmbeddingBuffer;

    public NeuralBackgroundWorker(
        ILogger<NeuralBackgroundWorker> logger,
        ChannelReader<string> intentReader,
        INeuralTokenizer tokenizer,
        INeuralModelService modelService,
        IEmbeddingGenerator embeddingGenerator,
        NeuralParameterRegistry registry,
        NeuralOscActionHandler actionHandler)
    {
        _intentReader = intentReader;
        _tokenizer = tokenizer;
        _embeddingGenerator = embeddingGenerator;
        _registry = registry;
        _actionHandler = actionHandler;
        _modelService = modelService;
        _logger = logger;

        _intentInterpreter = new DefaultSemanticIntentInterpreterImpl();
        _voiceEmbeddingBuffer = new float[embeddingGenerator.VectorSize];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _modelService.EnsureModelReadyAsync(stoppingToken);

        _logger.LogInformation("[NeuralEngine] WORKER IS LIVE AND LISTENING!"); // <-- Ser du denna i loggen?

        // STRICT: Zero-Allocation konsumtion via ChannelReader
        while (await _intentReader.WaitToReadAsync(stoppingToken))
        {
            while (_intentReader.TryRead(out var voiceText)) 
            {
                ProcessVoiceIntent(voiceText);
            }
        }
    }

    private void ProcessVoiceIntent(ReadOnlySpan<char> voiceText)
    {
        if (string.IsNullOrWhiteSpace(voiceText.ToString()) || voiceText.Contains('-')) return;

        // 1. TOKENIZE: Skapa tokens på stacken (Zero-alloc)
        Span<long> tokens = stackalloc long[128];
        int tokenCount = _tokenizer.TokenizeToSpan(voiceText.ToString(), tokens);

        if (tokenCount == 0) return;
    
        // 2. EMBEDDING: Generera röst-vektorn till vår förallokerade buffer
        _embeddingGenerator.GenerateEmbedding(tokens.Slice(0, tokenCount), _voiceEmbeddingBuffer);

        // 3. SEMANTIC MATCH: Hitta rätt Parameter ID i Ableton (SIMD-accelererat)
        // Vi kräver minst 70% matchning (0.7f) för att undvika "ghost triggers"
        int matchedId = _registry.FindBestMatch(_voiceEmbeddingBuffer, out float confidence);
        _logger.LogInformation("[NeuralEngine] Best match ID: {Id} (Confidence: {Score:P0})", matchedId, confidence);

        if (matchedId != -1)
        {
            // 4. INTERPRET: Vad ska göras? (Increase/Decrease/Set)
            if (_intentInterpreter.TryResolveValue(voiceText, out float val, out bool isRel))
            {
                // 5. ACTION: Skjut iväg OSC-paketet via din High-Performance Extension!
                _actionHandler.Execute(matchedId, val, isRel);
            }
        }
    }
}
