
namespace IntelligentAudio.Engine.Processors;

/// <summary>
/// DSP
/// </summary>
/// <param name="cutoffFrequency"></param>
/// <param name="sampleRate"></param>
public class SimpleHighPass(float cutoffFrequency = 80f, int sampleRate = 44100) : BaseAudioProcessor
{
    public override string Name => "HighPass Filter";

    // Enkel DC-blocker/HighPass-algoritm
    private float _lastInput = 0f;
    private float _lastOutput = 0f;
    private readonly float _alpha = CalculateAlpha(cutoffFrequency, sampleRate);

    private static float CalculateAlpha(float cutoff, int rate)
    {
        float dt = 1.0f / rate;
        float rc = 1.0f / (2.0f * MathF.PI * cutoff);
        return rc / (rc + dt);
    }

    protected override void OnProcess(Span<float> buffer)
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            float input = buffer[i];
            _lastOutput = _alpha * (_lastOutput + input - _lastInput);
            _lastInput = input;
            buffer[i] = _lastOutput;
        }
    }
}