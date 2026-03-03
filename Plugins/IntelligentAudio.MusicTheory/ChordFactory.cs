namespace IntelligentAudio.MusicTheory;

public class ChordFactory
{
    private static readonly Dictionary<string, int> NoteOffsets = new(StringComparer.OrdinalIgnoreCase)
    {
        { "C", 60 }, { "C#", 61 }, { "Db", 61 }, { "D", 62 }, { "D#", 63 },
        { "Eb", 63 }, { "E", 64 }, { "F", 65 }, { "F#", 66 }, { "Gb", 66 },
        { "G", 67 }, { "G#", 68 }, { "Ab", 68 }, { "A", 69 }, { "A#", 70 },
        { "Bb", 70 }, { "B", 71 }
    };

    public int[] Create(string root, string quality, float confidence)
    {
        if (!NoteOffsets.TryGetValue(root, out int baseNote))
            return Array.Empty<int>();

        int[] intervals = quality.ToLowerInvariant() switch
        {
            "major" or "maj" => [0, 4, 7],
            "minor" or "min" => [0, 3, 7],
            "major 7" or "maj7" => [0, 4, 7, 11],
            "minor 7" or "min7" => [0, 3, 7, 10],
            "dominant 7" or "7" => [0, 4, 7, 10],
            "sus4" => [0, 5, 7],
            "diminished" or "dim" => [0, 3, 6],
            _ => [0, 4, 7]
        };

        return intervals.Select(i => baseNote + i).ToArray();
    }

    public bool IsChord(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        // Kolla om strängen innehåller en tonart OCH en kvalitet
        var t = text.ToLowerInvariant();
        bool hasNote = NoteOffsets.Keys.Any(note => t.StartsWith(note.ToLowerInvariant()));
        bool hasQuality = t.Contains("major") || t.Contains("minor") || t.Contains("maj7") || t.Contains("dim");

        return hasNote && hasQuality;
    }
    public int[] Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return Array.Empty<int>();

        // 1. Normalisera texten (ta bort punkter, gör till gemener)
        var cleanText = text.Trim().Replace(".", "").ToLowerInvariant();

        // 2. Extrahera Root (leta efter C#, Db, C osv.)
        // Vi sorterar efter längd (t.ex. "C#" före "C") för att inte matcha fel
        string? foundRoot = NoteOffsets.Keys
            .OrderByDescending(k => k.Length)
            .FirstOrDefault(note => cleanText.StartsWith(note.ToLowerInvariant()));

        if (foundRoot == null) return Array.Empty<int>();

        // 3. Extrahera Quality (allt efter Root)
        // Ex: "c major" -> root "c", quality "major"
        string qualityPart = cleanText.Substring(foundRoot.Length).Trim();

        // Om ingen kvalitet anges, kör vi "major" som default
        if (string.IsNullOrEmpty(qualityPart)) qualityPart = "major";

        // 4. Anropa din befintliga Create-metod
        // Vi skickar 1.0f som confidence eftersom Whisper redan filtrerat detta
        return Create(foundRoot, qualityPart, 1.0f);
    }
}
