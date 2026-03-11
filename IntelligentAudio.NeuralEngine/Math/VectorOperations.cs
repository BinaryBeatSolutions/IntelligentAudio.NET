
namespace IntelligentAudio.NeuralEngine.Math;

public static class VectorOperations
{
    public static float CosineSimilarity(ReadOnlySpan<float> vectorA, ReadOnlySpan<float> vectorB)
    {
        if (vectorA.Length != vectorB.Length) return 0f;

        float dotProduct = 0f;
        float nA = 0f;
        float nB = 0f;

        // Använd Vector<float> för SIMD-optimering (.NET 10 profil)
        int vCount = Vector<float>.Count;
        int i = 0;

        for (; i <= vectorA.Length - vCount; i += vCount)
        {
            var va = new Vector<float>(vectorA.Slice(i));
            var vb = new Vector<float>(vectorB.Slice(i));

            dotProduct += Vector.Dot(va, vb);
            nA += Vector.Dot(va, va);
            nB += Vector.Dot(vb, vb);
        }

        // Resterande element utan LINQ/foreach
        for (; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            nA += vectorA[i] * vectorA[i];
            nB += vectorB[i] * vectorB[i];
        }

        return dotProduct / (MathF.Sqrt(nA) * MathF.Sqrt(nB));
    }
}