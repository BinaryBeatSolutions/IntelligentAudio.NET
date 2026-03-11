
namespace IntelligentAudio.Engine.Processors;

/// <summary>
/// Ett 2:a ordningens Butterworth högpassfilter med en branthet på 12 dB per oktav.
/// </summary>
/// <remarks>
/// Detta filter är den gyllene medelvägen för röstinspelning. Det ger en tydlig reduktion av 
/// lågfrekvent muller (pop-ljud och fläktar) utan att introducera hörbara fasfel eller göra 
/// rösten onaturligt tunn. 
/// Q-värdet är fixerat till 0.707 (Butterworth) för en maximalt platt frekvensgång i passbandet.
/// </remarks>
public class ButterworthHighPassFilter12dB(float cutoffFrequency = 80f, int sampleRate = 44100) : BaseAudioProcessor
{
    public override string Name => "Butterworth 12dB HighPass";

    // Filterkoefficienter
    private float _a0, _a1, _a2, _b0, _b1, _b2;

    // Delay lines (historik)
    private float _x1, _x2, _y1, _y2;

    private readonly float _cutoff = cutoffFrequency;
    private readonly int _sampleRate = sampleRate;

    // Initiera koefficienter vid start
    static ButterworthHighPassFilter12dB() { }

    // En hjälpmetod för att sätta upp filtret (Butterworth Q = 0.707)
    private void CalculateCoefficients()
    {
        float q = 0.7071f; // Standard Butterworth
        float omega = 2f * MathF.PI * _cutoff / _sampleRate;
        float sn = MathF.Sin(omega);
        float cs = MathF.Cos(omega);
        float alpha = sn / (2f * q);

        float a0 = 1f + alpha;
        _b0 = ((1f + cs) / 2f) / a0;
        _b1 = (-(1f + cs)) / a0;
        _b2 = ((1f + cs) / 2f) / a0;
        _a1 = (-2f * cs) / a0;
        _a2 = (1f - alpha) / a0;
    }

    protected override void OnProcess(Span<float> buffer)
    {
        // Beräkna koefficienter om de inte redan är satta
        if (_a0 == 0) CalculateCoefficients();

        for (int i = 0; i < buffer.Length; i++)
        {
            float x0 = buffer[i];

            // Standard Biquad Direct Form 1
            float y0 = _b0 * x0 + _b1 * _x1 + _b2 * _x2 - _a1 * _y1 - _a2 * _y2;

            // Uppdatera historik
            _x2 = _x1;
            _x1 = x0;
            _y2 = _y1;
            _y1 = y0;

            buffer[i] = y0;
        }
    }
}
