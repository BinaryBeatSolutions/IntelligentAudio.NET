
namespace IntelligentAudio.Infrastructure.Services;

public sealed class NexusStorageEngine
{
    // Fast, orubblig adress internt i motorn
    private static readonly string _indexFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IntelligentAudio.NET",
        "Cache",
        "NexusIndex.ian"
    );

    private readonly ILogger _logger;
    private readonly ICloudProvider _cloudProvider;
    private const int _maxInitialEntries = 1_000_000;
    private const long _headerSize = 64;
    private const long _entrySize = 24; // Storleken på RegistryEntry

    public static string FilePath => _indexFilePath;

    public NexusStorageEngine(ILogger<ICloudProvider> logger, ICloudProvider cloudProvider)
    {
        _logger = logger;
        _cloudProvider = cloudProvider;
    }


    /// <summary>
    /// Initialiserar filstrukturen och reserverar plats (Slack Space).
    /// Denna 'awaitas' vid systemstart.
    /// </summary>
    public async Task InitializeAsync()
    {
        _logger.LogInformation("[NEXUS] Storage Initialized at: {Path}", _indexFilePath);
        _logger.LogInformation("[NEXUS] Allocated Slack Space for {Count} entries.", _maxInitialEntries);


        if (File.Exists(_indexFilePath)) return;

        Directory.CreateDirectory(Path.GetDirectoryName(_indexFilePath)!);

        // Vi använder standard FileStream - helt safe.
        using var fs = new FileStream(_indexFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);

        long indexOffset = _headerSize;
        long dataOffset = _headerSize + (_maxInitialEntries * _entrySize);

        var header = new NexusRegistryHeader(
            magicBytes: 0x49414E4558555321,
            version: 1,
            entryCount: 0,
            indexOffset: indexOffset,
            dataOffset: dataOffset
        );

        // Reservera plats på disken (Zero-fill) - Safe operation
        fs.SetLength(dataOffset + (1024 * 1024));

        // SKRIV HEADERN UTAN UNSAFE:
        // MemoryMarshal.CreateReadOnlySpan låter oss se structen som bytes utan kopiering
        ReadOnlySpan<NexusRegistryHeader> headerSpan = MemoryMarshal.CreateReadOnlySpan(ref header, 1);
        ReadOnlySpan<byte> headerBytes = MemoryMarshal.AsBytes(headerSpan);

        await fs.WriteAsync(headerBytes.ToArray()); // ToArray här är OK, körs bara en gång vid init
        await fs.FlushAsync();
    }


    /// <summary>
    /// Används för att "patcha" in data från t.ex. en ny Vercel-download
    /// </summary>
    public async ValueTask<NexusResource> AppendCloudDataAsync(byte[] data)
    {
        // Logik för att skriva i slutet av data-segmentet (Beskrivet tidigare)
        // Returnerar koordinaterna så att Indexet kan uppdateras
        return new NexusResource(0, data.Length, true);
    }

    public async Task SyncParameterAsync(ParameterKey key)
    {
        // Infrastructure sköter logiken: "Hämta -> Validera -> Patcha MMF"
        byte[] data = await _cloudProvider.DownloadPresetAsync(key);
        await AppendCloudDataAsync(data);
    }

    /// <summary>
    /// TESTS for dashboard
    /// </summary>
    /// <returns></returns>
    public async Task MassSeedTestDataAsync(int count = 10000)
    {
        // 1. Skapa test-data i RAM först
        var entries = new List<Contracts.Models.RegistryEntry>(count);
        for (uint i = 0; i < (uint)count; i++)
        {
            // Skapa unika nycklar (Audio, Provider 1, Entity i, Sub 0)
            var key = ParameterKey.Create(0, 1, (ushort)i, 0);
            entries.Add(new RegistryEntry(key, i * 4, 4));
        }

        // 2. KRITISKT: Sortera listan efter ulong-värdet för Binary Search
        entries.Sort((a, b) => a.Key.Value.CompareTo(b.Key.Value));

        // 3. Skriv hela blocket asynkront till disk
        using var fs = new FileStream(_indexFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite);

        // Gå till början av Index (efter headern på 64 bytes)
        fs.Seek(64, SeekOrigin.Begin);

        // Konvertera listan till bytes via Span (Safe & Fast)
        var span = CollectionsMarshal.AsSpan(entries);
        await fs.WriteAsync(MemoryMarshal.AsBytes(span).ToArray());

        // 4. Uppdatera EntryCount (long) i Headern (Offset 12)
        fs.Seek(12, SeekOrigin.Begin);
        await fs.WriteAsync(BitConverter.GetBytes((long)count));

        await fs.FlushAsync();
    }

}

