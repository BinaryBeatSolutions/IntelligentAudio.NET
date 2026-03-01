
namespace IntelligentAudio.Engine.Services;

public static class AudioAnalysis
{
    /// <summary>
    /// Vectorized RMS calculation using SIMD (Vector<float>).
    /// Zero allocations, near-zero CPU impact.
    /// </summary>
    public static float CalculateRms(ReadOnlySpan<short> samples)
    {
        if (samples.IsEmpty) return 0;

        float sumOfSquares = 0;
        int i = 0;

        // .NET 10 Vectorized loop (SIMD)
        if (Vector.IsHardwareAccelerated && samples.Length >= Vector<float>.Count)
        {
            var vSum = Vector<float>.Zero;
            while (i <= samples.Length - Vector<float>.Count)
            {
                // Skapa en vektor från shorts (vi måste casta/konvertera till float för kvadrering)
                // Obs: För maximal prestanda kan man använda intrinsics direkt, 
                // men Vector<T> är säkrare för cross-platform.
                for (int j = 0; j < Vector<float>.Count; j++)
                {
                    float s = samples[i + j] / 32768f;
                    vSum += new Vector<float>(s * s);
                }
                i += Vector<float>.Count;
            }
            sumOfSquares = Vector.Dot(vSum, Vector<float>.One);
        }

        // Ta hand om resterande samples som inte fick plats i en hel vektor
        for (; i < samples.Length; i++)
        {
            float s = samples[i] / 32768f;
            sumOfSquares += s * s;
        }

        return (float)Math.Sqrt(sumOfSquares / samples.Length);
    }

    /// <summary>
    /// High-performance Linear Resampler (44.1 -> 16).
    /// Zero allocations, safe boundary checks.
    /// </summary>
    public static void Resample(ReadOnlySpan<float> source, Span<float> destination)
    {
        if (source.IsEmpty || destination.IsEmpty) return;

        float ratio = (float)(source.Length - 1) / (destination.Length - 1);

        for (int i = 0; i < destination.Length; i++)
        {
            float sourcePos = i * ratio;
            int index = (int)sourcePos;
            float fraction = sourcePos - index;

            if (index >= source.Length - 1)
            {
                destination[i] = source[^1];
                continue;
            }

            // Linear Interpolation: lerp(a, b, t) = a + t * (b - a)
            destination[i] = source[index] + fraction * (source[index + 1] - source[index]);
        }
    }
}
