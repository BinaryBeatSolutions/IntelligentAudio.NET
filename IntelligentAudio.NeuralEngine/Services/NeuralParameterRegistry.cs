
using IntelligentAudio.NeuralEngine.Math;
using System.Buffers;

namespace IntelligentAudio.NeuralEngine.Services;

public sealed class NeuralParameterRegistry : IDisposable
{
    private readonly int _vectorSize;
    private float[] _linearStorage; // Linjärt minne för SIMD-vänlig sökning
    private int[] _parameterIds;    // Motsvarande ID-nummer på samma index
    private int _count;

    public NeuralParameterRegistry(int vectorSize, int initialCapacity = 256)
    {
        _vectorSize = vectorSize;
        _linearStorage = ArrayPool<float>.Shared.Rent(initialCapacity * vectorSize);
        _parameterIds = ArrayPool<int>.Shared.Rent(initialCapacity);
    }

    /// <summary>
    /// Registrerar en ny parameter och dess embedding.
    /// Sker under "Discovery Phase", ej i hot path.
    /// </summary>
    public void RegisterParameter(int id, ReadOnlySpan<float> embedding)
    {
        // Kontrollera kapacitet och expandera vid behov (ej allokeringsfritt, men sällsynt)
        EnsureCapacity(_count + 1);

        int offset = _count * _vectorSize;
        embedding.CopyTo(_linearStorage.AsSpan(offset, _vectorSize));
        _parameterIds[_count] = id;
        _count++;
    }

    /// <summary>
    /// Söker efter bäst matchning med SIMD.
    /// </summary>
    public int FindBestMatch(ReadOnlySpan<float> voiceVector, float threshold = 0.75f)
    {
        int bestId = -1;
        float bestScore = -1f;

        // STRICT: Manuell loop för att garantera SIMD-optimering och noll allokering
        for (int i = 0; i < _count; i++)
        {
            int offset = i * _vectorSize;
            var paramVector = _linearStorage.AsSpan(offset, _vectorSize);

            float score = VectorOperations.CosineSimilarity(voiceVector, paramVector);

            if (score > bestScore && score >= threshold)
            {
                bestScore = score;
                bestId = _parameterIds[i];
            }
        }

        return bestId;
    }

    private void EnsureCapacity(int min) { /* Implementera ArrayPool-expansion här */ }

    public void Dispose()
    {
        ArrayPool<float>.Shared.Return(_linearStorage);
        ArrayPool<int>.Shared.Return(_parameterIds);
    }
}