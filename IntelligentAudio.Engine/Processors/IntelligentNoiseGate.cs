
namespace IntelligentAudio.Engine.Processors;

public class IntelligentNoiseGate : BaseAudioProcessor
{
    public override string Name => "Intelligent Sidechained Gate";

    // Filter för detektering (Sidechain)
    private readonly ButterworthHighPassFilter24dB _sidechainFilter;

    // Gate-parametrar
    private float _threshold = 0.01f; // Justera efter behov
    private float _envelope = 0f;
    private float _attack = 0.99f;  // Hur snabbt den öppnar
    private float _release = 0.999f; // Hur snabbt den stänger

    public IntelligentNoiseGate(float threshold = 0.01f, float cutoff = 80f, int sampleRate = 44100)
    {
        _threshold = threshold;
        _sidechainFilter = new ButterworthHighPassFilter24dB(cutoff, sampleRate);
    }

    protected override void OnProcess(Span<float> buffer)
    {
        // Vi behöver en kopia för sidechain-analys så vi inte förstör originalet direkt
        Span<float> sidechainBuffer = stackalloc float[buffer.Length];
        buffer.CopyTo(sidechainBuffer);

        // 1. Filtrera sidechain-signalen (ta bort muller som inte ska trigga gaten)
        _sidechainFilter.Process(sidechainBuffer);

        for (int i = 0; i < buffer.Length; i++)
        {
            // 2. Envelope Follower (hitta nivån på den filtrerade signalen)
            float absInput = MathF.Abs(sidechainBuffer[i]);

            if (absInput > _envelope)
                _envelope = _envelope * (1.0f - _attack) + absInput * _attack;
            else
                _envelope = _envelope * _release;

            // 3. Gate-logik: Om energin i sidechain är under threshold, muta originalet
            if (_envelope < _threshold)
            {
                buffer[i] = 0f;
            }
            // Annars låter vi buffer[i] vara (originalet passerar)
        }
    }
}
