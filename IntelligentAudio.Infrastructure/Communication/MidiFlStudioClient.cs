using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;

namespace IntelligentAudio.Infrastructure.Communication;

public class MidiFlStudioClient(Guid clientId) : IDawClient
{
    public Guid ClientId => clientId;
    public string Name => "FL Studio (MIDI)";
    public int Port => 0; // MIDI använder port-namn snarare än UDP-portar

    public async Task SendChordAsync(ChordInfo chord)
    {
        // Framtida logik: Skicka MIDI Note On/Off via DryWetMidi
        await Task.CompletedTask;
    }

    public async Task SendCommandAsync(DawCommand command)
    {
        // Mappa DawAction till MIDI CC eller Machine Control (MMC)
        await Task.CompletedTask;
    }

    public void Dispose() { }
}
