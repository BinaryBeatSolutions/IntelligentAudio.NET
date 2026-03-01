
using Whisper.net.Ggml;

namespace IntelligentAudio.Server.Extensions;


public class PathResolver
{

    private readonly ILogger<PathResolver> _logger;

    public PathResolver(ILogger<PathResolver> logger)
    {

        _logger = logger;
    }

    public string Resolve(string relativePath)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var path = Path.GetFullPath(Path.Combine(baseDir, relativePath));
        _logger.LogDebug("[IntelligentAudio.NET] Resolved path {Relative} -> {Absolute}", relativePath, path);
        return path;
    }
}



//public static class PathResolver : IPathResolver
//{
//    /// <summary>
//    /// Returnerar sökvägen till en specifik Whisper‑modell.  
//    /// Laddar ner modellen om den inte redan finns i användarens AppData‑mapp.
//    /// </summary>
//    /// <param name="modelName">Filnamn, t.ex. "ggml-tiny.en.bin"</param>
//    /// <returns>Fullständig filväg.</returns>
//    public static async Task<string> GetModelPathAsync(string modelName)
//    {
//        // 1️⃣ Skapa sökvägen i %LOCALAPPDATA%\BinaryBeat\Models
//        var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
//        var folder = Path.Combine(appData, "BinaryBeat", "Models");

//        if (!Directory.Exists(folder))
//            Directory.CreateDirectory(folder);

//        var path = Path.Combine(folder, modelName);

//#if DEBUG
//        Console.WriteLine($"[PathResolver] Modell‑väg: {path}");
//#endif

//        // 2️ Om filen inte finns – ladda ner den med WhisperGgmlDownloader
//        if (!File.Exists(path))
//        {
//            await DownloadModelAsync(modelName, path);
//        }

//        return path;
//    }

//    /// <summary>
//    /// Laddar ner en modell från Whisper.GgmlDownloader och sparar den på angiven plats.
//    /// </summary>
//    private static async Task DownloadModelAsync(string modelName, string targetPath)
//    {
//#if DEBUG
//        Console.WriteLine($"[BinaryBeat] Laddar ner {modelName} via GgmlDownloader…");
//#endif

//        // 3️⃣ Välj rätt GgmlType baserat på filnamnet.  
//        // Du kan lägga till en egen enum eller ett mapp‑system för att automatisera detta.
//        var type = MapModelNameToGgmlType(modelName);

//        using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(type);
//        using var fileWriter = File.Create(targetPath);   // Skapar (eller rensar) filen
//        await modelStream.CopyToAsync(fileWriter);

//#if DEBUG
//        Console.WriteLine("[BinaryBeat] Nedladdning slutförd.");
//#endif
//    }

//    /// <summary>
//    /// En enkel mapp‑funktion som konverterar filnamn till GgmlType‑enum.
//    /// </summary>
//    private static GgmlType MapModelNameToGgmlType(string modelName)
//    {
//        // Exempel: "ggml-tiny.en.bin"  →  GgmlType.TinyEn
//        if (modelName.Contains("tiny", StringComparison.OrdinalIgnoreCase))
//            return GgmlType.TinyEn;
//        if (modelName.Contains("base", StringComparison.OrdinalIgnoreCase))
//            return GgmlType.BaseEn;          // Existerar om du har en sådan i Whisper.NET
//        if (modelName.Contains("large", StringComparison.OrdinalIgnoreCase))
//            return GgmlType.LargeV2;

//        // Fallback – Tiny som standard
//        return GgmlType.TinyEn;
//    }
//}

