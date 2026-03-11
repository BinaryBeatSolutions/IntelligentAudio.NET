
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace IntelligentAudio.NeuralEngine.Services;

public sealed class DefaultEmbeddingGeneratorImpl : IEmbeddingGenerator, IDisposable
{
    private InferenceSession? _session;
    private int _vectorSize;
    private string? _inputName;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private string? _modelPath;

    public int VectorSize => _vectorSize;

    public DefaultEmbeddingGeneratorImpl(string modelPath)
    {
        _modelPath = modelPath;
    }

    /// <summary>
    /// Genererar en embedding utan att allokera på heapen.
    /// </summary>
    /// <param name="tokens">Input tokens från Tokenizern</param>
    /// <param name="destination">Förallokerad buffer (Span) för resultatet</param>
    public void GenerateEmbedding(ReadOnlySpan<long> tokens, Span<float> destination)
    {
        if (_session == null)
        {
            _initLock.Wait();
            try
            {
                // Dubbelkoll efter låset (Double-checked locking)
                if (_session == null)
                {
                    var options = new SessionOptions
                    {
                        ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
                        GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
                        // Begränsa trådar så att AI:n inte "stjäl" från Audio Engine (Audio Thread Priority)
                        IntraOpNumThreads = 1
                    };

                    options.AppendExecutionProvider_CPU(0);
                    _session = new InferenceSession(_modelPath, options);
                    _inputName = _session.InputMetadata.Keys.First();
                    _vectorSize = _session.OutputMetadata.First().Value.Dimensions[1]; // Ofta 384 eller 768
                }
            }
            finally
            {
                _initLock.Release();
            }
        }

        int length = tokens.Length;
        // ONNX vill ha int[] för dimensioner, inte long[]
        ReadOnlySpan<int> shape = stackalloc int[] { 1, length };

        // 1. Input IDs
        // Vi mappar Memory<long> direkt (via ToArray för enkelhet nu)
        var inputIds = new DenseTensor<long>(tokens.ToArray(), shape);

        // 2. Attention Mask (Ettor)
        var maskArray = new long[length];
        Array.Fill(maskArray, 1L);
        var attentionMask = new DenseTensor<long>(maskArray, shape);

        // 3. Token Type IDs (Nollor)
        // OBS: Dessa ska vara long (int64) för de flesta MiniLM modeller, 
        // men kolla om din modell kräver int32. Vi kör long här:
        var typeIdsArray = new long[length];
        var tokenTypeIds = new DenseTensor<long>(typeIdsArray, shape);

        var inputs = new List<NamedOnnxValue>
    {
        NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
        NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
        NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)
    };

        using var results = _session.Run(inputs);

        // Hämta utdata-tensorn (första utsignalen)
        var outputTensor = results.First().AsTensor<float>();

        // Kopiera direkt från Tensor till din förallokerade destination (Span)
        // Detta undviker att skapa en ny float[]
        for (int i = 0; i < _vectorSize; i++)
        {
            destination[i] = outputTensor[0, i];
        }
    }

    public void Dispose()
    {
        _session?.Dispose();
    }
}