
namespace IntelligentAudio.Engine.Processors;

/// <summary>
/// Optimerar ljudsignalen specifikt för taligenkänning med OpenAI Whisper.
/// </summary>
/// <remarks>
/// Denna processor utför tre kritiska steg för att maximera träffsäkerheten (Word Error Rate):
/// 1. **Pre-Emphasis**: Högpassfiltrering (diskantboost) som förstärker konsonanter och högfrekventa 
///    detaljer, vilket hjälper Whisper att identifiera ordgränser.
/// 2. **RMS-analys**: Använder <see cref="AudioMath.CalculateRms"/> för att mäta signalens energi.
/// 3. **Automatic Gain Control (AGC)**: Normaliserar volymen till en målnivå på ca -16dBFS. 
///    Detta säkerställer att Whisper får en stabil signal oavsett om användaren pratar tyst eller högt.
/// </remarks>
public class WhisperPreProcessor : BaseAudioProcessor
{
    public override string Name => "Whisper Audio Optimizer";

    private float _lastSample = 0f;
    private float _currentRms = 0f;
    private const float TargetRms = 0.15f; // Optimal nivå för Whisper (-16dBFS)
    private const float PreEmphasis = 0.97f; // Lyfter konsonanter/tydlighet

    protected override void OnProcess(Span<float> buffer)
    {
        // 1. Använd din befintliga AudioMath för att mäta volymen
        float rms = AudioMath.CalculateRms(buffer);

        // Glidande medelvärde (Smoothing) för att undvika volym-pumpande
        _currentRms = (_currentRms * 0.8f) + (rms * 0.2f);

        // Beräkna gain. Om det är för tyst (under -60dB), boosta inte bruset.
        float gain = (_currentRms < 0.001f) ? 1.0f : TargetRms / _currentRms;
        gain = Math.Min(gain, 8.0f); // Max 18dB boost för att inte förstöra signalen

        for (int i = 0; i < buffer.Length; i++)
        {
            float current = buffer[i];

            // 2. Pre-Emphasis filter (y[n] = x[n] - 0.97 * x[n-1])
            // Detta gör talet extremt mycket tydligare för Whisper
            float highPassed = current - PreEmphasis * _lastSample;
            _lastSample = current;

            // 3. Applicera den beräknade volymjusteringen
            buffer[i] = highPassed * gain;
        }
    }
}
