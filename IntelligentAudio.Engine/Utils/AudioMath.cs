

namespace IntelligentAudio.Engine.Utils;

public static class AudioMath
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateRms(ReadOnlySpan<short> samples)
    {
        if (samples.IsEmpty) return 0f;

        // BUG FIX: samples.Length is already the count of shorts. 
        // No need for 'sizeof(short)' division.
        double sumSquares = 0;

        // Manual loop for maximum control (SIMD could be added later)
        for (int i = 0; i < samples.Length; i++)
        {
            float sample = samples[i] / 32768f; // Normalize to float
            sumSquares += sample * sample;
        }

        return (float)Math.Sqrt(sumSquares / samples.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Resample(ReadOnlySpan<float> source, Span<float> destination)
    {
        if (source.IsEmpty || destination.IsEmpty) return;

        // Linear interpolation ratio
        float ratio = (float)(source.Length - 1) / (destination.Length - 1);

        for (int i = 0; i < destination.Length; i++)
        {
            float sourcePos = i * ratio;
            int index = (int)sourcePos;
            float fraction = sourcePos - index;

            // Safe boundary check
            if (index >= source.Length - 1)
            {
                destination[i] = source[^1];
                continue;
            }

            // Lerp: a + t * (b - a)
            destination[i] = source[index] + fraction * (source[index + 1] - source[index]);
        }
    }
}
