

namespace IntelligentAudio.Contracts.Interfaces;

/// <summary>
/// Implementing with the future in min to support most or all DAW's
/// </summary>
public interface IDawClient : IDisposable
{
    Guid ClientId { get; }
    string Name { get; } // "Ableton", "FL Studio", "Logic"

    Task SendChordAsync(ChordInfo chord);
    Task SendCommandAsync(DawCommand command); // DawCommand

    public IDawClient CreateClient(Guid id, string dawType, int port)
    {
        return dawType.ToLower() switch
        {
            "ableton" => new OscAbletonClient(id, "127.0.0.1", port),
            "flstudio" => new MidiFlStudioClient(id), // Framtida implementation
            "vst_generic" => new VstBridgeClient(id, port), // För Logic/Cubase
            _ => throw new NotSupportedException($"Support for {dawType} coming soon!")
        };
    }
}