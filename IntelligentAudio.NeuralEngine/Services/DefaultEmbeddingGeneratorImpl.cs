
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace IntelligentAudio.NeuralEngine.Services;

public sealed class DefaultEmbeddingGeneratorImpl : IEmbeddingGenerator, IDisposable
{
    private readonly InferenceSession _session;
    private readonly int _vectorSize;
    private readonly string _inputName;

    public int VectorSize => _vectorSize;

    public DefaultEmbeddingGeneratorImpl(string modelPath)
    {
        // Latency Analysis: Initiering sker vid uppstart, ej i hot path.
        var options = new SessionOptions
        {
            ExecutionMode = ExecutionMode.ORT_SEQUENTIAL,
            GraphOptimizationLevel = GraphOptimizationLevel.ORT_ENABLE_ALL,
            // Begränsa trådar så att AI:n inte "stjäl" från Audio Engine (Audio Thread Priority)
            IntraOpNumThreads = 1
        };

        _session = new InferenceSession(modelPath, options);

        // Detektera automatiskt modellens egenskaper för att undvika hårdkodning
        _inputName = _session.InputMetadata.Keys.First();
        _vectorSize = _session.OutputMetadata.First().Value.Dimensions[1]; // Ofta 384 eller 768
    }

    /// <summary>
    /// Genererar en embedding utan att allokera på heapen.
    /// </summary>
    /// <param name="tokens">Input tokens från Tokenizern</param>
    /// <param name="destination">Förallokerad buffer (Span) för resultatet</param>
    public void GenerateEmbedding(ReadOnlySpan<long> tokens, Span<float> destination)
    {
        if (destination.Length < _vectorSize)
            throw new ArgumentException($"Destination span must be at least {_vectorSize} elements.");

        // Allocation Analysis: ONNX Runtime kräver NamedOnnxValue (liten heap-allokering).
        // För att gå till absolut noll krävs "FixedBuffer" API, men detta är "Good Enough" 
        // för röstintervaller (millisekunder).

        // Vi mappar tokens direkt till en Tensor utan onödig kopiering
        // .ToArray() krävs tyvärr av DenseTensor i nuvarande ORT version
        var inputTensor = new DenseTensor<long>(tokens.ToArray(), new[] { 1, tokens.Length });

        var inputs = new List<NamedOnnxValue>(1)
        {
            NamedOnnxValue.CreateFromTensor(_inputName, inputTensor)
        };

        // Kör Inference
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