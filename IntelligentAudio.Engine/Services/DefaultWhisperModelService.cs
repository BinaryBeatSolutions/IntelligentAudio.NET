
namespace IntelligentAudio.Engine.Services;

using Whisper.net;
using Whisper.net.Ggml;

/// <summary>
/// Handles model loading, inc. download, (block system until finished) if no model exits.
/// </summary>
/// <param name="logger"></param>
public sealed class DefaultWhisperModelService(ILogger<DefaultWhisperModelService> logger) : IDisposable
{
    private WhisperFactory? _factory;
    private readonly string _baseDataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "IntelligentAudio.NET",
        "Models");

    public bool IsReady => _factory != null;

    public async Task EnsureModelReadyAsync(WhisperModelType modelType, CancellationToken ct)
    {
        // 1. Map to Whisper.net types
        var ggmlType = MapModelType(modelType);
        var fileName = $"ggml-{modelType.ToString().ToLower()}.bin";
        var fullPath = Path.Combine(_baseDataPath, fileName);

        // 2. Secure Directory Creation (cross-platform safe)
        if (!Directory.Exists(_baseDataPath))
        {
            logger.LogInformation("[Whisper] Creating secure storage at: {path}", _baseDataPath);
            Directory.CreateDirectory(_baseDataPath);
        }

        // 3. Auto-Download with Progress
        if (!File.Exists(fullPath))
        {
            logger.LogWarning("[Whisper] Model {type} missing. Initiating secure download...", modelType);

            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(MapModelType(modelType));
            using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

            // TODO: Hook into your IEventAggregator here for OSC Progress
            await modelStream.CopyToAsync(fileStream, ct);
            logger.LogInformation("[Whisper] Download complete: {file}", fileName);
        }

        // 4. Load Factory (Hot-swap support)
        _factory?.Dispose();
        _factory = WhisperFactory.FromPath(fullPath);

        //logger.LogInformation("[IntelligentAudio.NET] Model {type} loaded successfully.{fullPath}", modelType, fullPath);
    }

    // Overkill, but in future if we want to change model, we can do it winthin tha DAW
    // depending on the need of a larger model.
    private GgmlType MapModelType(WhisperModelType type) => type switch
    {
        WhisperModelType.Tiny => GgmlType.TinyEn,
        WhisperModelType.Base => GgmlType.Base,
        WhisperModelType.Small => GgmlType.Small,
        WhisperModelType.Medium => GgmlType.Medium,
        _ => GgmlType.Base
    };

    /// <summary>
    /// Creates WhisperProcessor
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public WhisperProcessor CreateProcessor()
        => _factory?.CreateBuilder()
            .WithLanguage("en") //Use of model could depend on the use. Below for chords only.
            .WithPrompt("Musical chords: C, C#, Db, D, Eb, E, F, F#, G, Ab, A, Bb, B. Major, Minor, Maj7, m7, Dominant, Sus4, Diminished.")
            .Build()
           ?? throw new InvalidOperationException("Brain not initialized. Call EnsureModelReadyAsync first.");

    public void Dispose() => _factory?.Dispose();
}
