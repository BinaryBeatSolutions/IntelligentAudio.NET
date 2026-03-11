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

    public static int GetRootOffset(ReadOnlySpan<char> root) => root switch
    {
        "C" or "c" => 60,
        "C#" or "c#" or "Db" or "db" => 61,
        "D" or "d" => 62,
        "D#" or "d#" or "Eb" or "eb" => 63,
        "E" or "e" => 64,
        "F" or "f" => 65,
        "F#" or "f#" or "Gb" or "gb" => 66,
        "G" or "g" => 67,
        "G#" or "g#" or "Ab" or "ab" => 68,
        "A" or "a" => 69,
        "A#" or "a#" or "Bb" or "bb" => 70,
        "B" or "b" => 71,
        _ => -1
    };

    // Returnerar antal noter skrivna till resultBuffer. 
    // Använder ReadOnlySpan för att undvika .ToLower() allokeringar.
    public static int CreateToSpan(ReadOnlySpan<char> root, ReadOnlySpan<char> quality, Span<int> resultBuffer)
    {
        int baseNote = GetRootOffset(root);
        if (baseNote == -1 || resultBuffer.Length < 4) return 0;

        // Statiska intervaller för att undvika array-allokering
        ReadOnlySpan<int> intervals = quality switch
        {
            "major" or "maj" => [0, 4, 7],
            "minor" or "min" => [0, 3, 7],
            "maj7" or "major 7" => [0, 4, 7, 11],
            "min7" or "minor 7" => [0, 3, 7, 10],
            "7" or "dominant" => [0, 4, 7, 10],
            "sus4" => [0, 5, 7],
            "dim" or "diminished" => [0, 3, 6],
            _ => [0, 4, 7] // Default Major
        };

        for (int i = 0; i < intervals.Length; i++)
        {
            resultBuffer[i] = baseNote + intervals[i];
        }

        return intervals.Length;
    }

    public static int ParseAndCreate(ReadOnlySpan<char> input, Span<int> resultBuffer)
    {
        if (input.IsEmpty) return 0;

        // 1. Hitta var noten slutar och kvaliteten börjar
        // Ex: "C#major" -> Root: "C#", Quality: "major"
        int splitIndex = 1;
        if (input.Length > 1 && (input[1] == '#' || input[1] == 'b' || input[1] == 'B'))
        {
            splitIndex = 2;
        }

        // Skapa slices utan att allokera nya strängar
        ReadOnlySpan<char> root = input[..splitIndex];
        ReadOnlySpan<char> quality = input.Length > splitIndex
            ? input[splitIndex..].Trim()
            : "major".AsSpan(); // Default

        // 2. Nu har vi våra 3 args!
        return CreateToSpan(root, quality, resultBuffer);
    }
}
