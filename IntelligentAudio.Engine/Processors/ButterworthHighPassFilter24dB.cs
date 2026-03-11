
namespace IntelligentAudio.Engine.Processors;

/// <summary>
/// Ett 4:e ordningens Butterworth högpassfilter med en branthet på 24 dB per oktav.
/// </summary>
/// <remarks>
/// Genom att kaskadkoppla två biquad-steg uppnås en mycket brant avrullning som effektivt 
/// isolerar talet från kraftigt bakgrundsbrus, jordbrum (50/60Hz) eller mekaniska stötar.
/// Idealisk som sidechain-detektor för NoiseGates eller som en aggressiv pre-processor 
/// inför Whisper-inferens i bullriga miljöer.
/// </remarks>
public class ButterworthHighPassFilter24dB : BaseAudioProcessor
{
    public override string Name => "Butterworth 24dB HighPass";

    private BiquadState _s1, _s2;

    public ButterworthHighPassFilter24dB(float cutoffFrequency = 80f, int sampleRate = 44100)
    {
        // Steg 1: Q ≈ 0.5412, Steg 2: Q ≈ 1.3066 för 4:e ordningens Butterworth
        _s1 = CalculateCoefficients(cutoffFrequency, 0.5411961f, sampleRate);
        _s2 = CalculateCoefficients(cutoffFrequency, 1.3065630f, sampleRate);
    }

    private static BiquadState CalculateCoefficients(float cutoff, float q, int rate)
    {
        float omega = 2f * MathF.PI * cutoff / rate;
        float sn = MathF.Sin(omega);
        float cs = MathF.Cos(omega);
        float alpha = sn / (2f * q);
        float a0 = 1f + alpha;

        return new BiquadState
        {
            B0 = ((1f + cs) / 2f) / a0,
            B1 = (-(1f + cs)) / a0,
            B2 = ((1f + cs) / 2f) / a0,
            A1 = (-2f * cs) / a0,
            A2 = (1f - alpha) / a0
        };
    }

    protected override void OnProcess(Span<float> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            // Processa genom båda stegen sekventiellt
            float x = buffer[i];

            // Steg 1
            float y1 = _s1.B0 * x + _s1.B1 * _s1.X1 + _s1.B2 * _s1.X2 - _s1.A1 * _s1.Y1 - _s1.A2 * _s1.Y2;
            _s1.X2 = _s1.X1; _s1.X1 = x; _s1.Y2 = _s1.Y1; _s1.Y1 = y1;

            // Steg 2
            float y2 = _s2.B0 * y1 + _s2.B1 * _s2.X1 + _s2.B2 * _s2.X2 - _s2.A1 * _s2.Y1 - _s2.A2 * _s2.Y2;
            _s2.X2 = _s2.X1; _s2.X1 = y1; _s2.Y2 = _s2.Y1; _s2.Y1 = y2;

            buffer[i] = y2;
        }
    }

    private struct BiquadState
    {
        public float B0, B1, B2, A1, A2;
        public float X1, X2, Y1, Y2;
    }
}

