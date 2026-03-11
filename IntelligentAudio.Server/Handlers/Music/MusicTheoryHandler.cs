
using IntelligentAudio.MusicTheory; 
using IntelligentAudio.Contracts.Interfaces;

namespace IntelligentAudio.Server.Handlers.Music;

public class MusicTheoryHandler(ChordFactory chordFactory, ILogger<MusicTheoryHandler> logger) : IIntentHandler
{
    public bool CanHandle(string text) =>
        !string.IsNullOrWhiteSpace(text) &&
        text.Length < 20 && // <--- Ackord är sällan långa meningar
        chordFactory.IsChord(text);

    public async ValueTask HandleAsync(string text, CancellationToken ct)
    {
        // Parse sköter nu uppdelningen mellan "C" och "Major"
        int[] midiNotes = chordFactory.Parse(text);

        if (midiNotes.Length > 0)
        {
            logger.LogInformation("[MusicTheory] Success! String '{text}' became MIDI: {notes}",
                text, string.Join(", ", midiNotes));

            // HÄR: Nästa steg blir att skicka dessa till din OSC-tjänst
            // await _oscService.SendChordAsync(midiNotes);
        }

        await Task.CompletedTask;
    }
}