

namespace IntelligentAudio.Engine.Processors;

public sealed class NoiseGateProcessor
{
    private DateTime _lastActiveTime = DateTime.MinValue;

    public float Threshold { get; set; } = 0.04f; // Justerbar känslighet
    public TimeSpan HoldTime { get; set; } = TimeSpan.FromMilliseconds(500);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsOpen(ReadOnlySpan<short> samples)
    {
        // 1. Beräkna RMS (använder vår blixtsnabba AudioMath)
        float rms = AudioMath.CalculateRms(samples);

        // 2. Kolla om vi är över tröskelvärdet
        if (rms >= Threshold)
        {
            _lastActiveTime = DateTime.UtcNow;
            return true;
        }

        // 3. Hysteresis: Håll gaten öppen en stund efter att ljudet tystnat
        return (DateTime.UtcNow - _lastActiveTime) < HoldTime;
    }
}