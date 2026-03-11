
using IntelligentAudio.Contracts;
using Microsoft.Extensions.Logging;
using System.Buffers;

namespace IntelligentAudio.NeuralEngine.Services;

public sealed class DefaultNeuralModelServiceImpl : INeuralModelService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;   
    private readonly string _modelPath;
    private readonly string _vocabPath;

    public DefaultNeuralModelServiceImpl(ILogger<DefaultNeuralModelServiceImpl> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        // Vi utgår från den gemensamma rot-mappen för IntelligentAudio.NET
        string baseDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "IntelligentAudio.NET",
            "Models");

        // Neural Engine-specifika filer under Models-mappen
        _modelPath = Path.Combine(baseDir, "NeuralHarmonic", "embeddings.onnx");
        _vocabPath = Path.Combine(baseDir, "NeuralHarmonic", "vocab.txt");
    }

    public string ModelPath => _modelPath;
    public string VocabPath => _vocabPath;

    public async ValueTask EnsureModelReadyAsync(CancellationToken ct)
    {
        if (File.Exists(_modelPath) && File.Exists(_vocabPath)) return;

        // Hämta mappen för filerna (t.ex. ...\Models\NeuralHarmonic)
        string? modelDir = Path.GetDirectoryName(_modelPath);

        if (!string.IsNullOrEmpty(modelDir))
        {
            // Detta skapar hela sökvägen (inkl. IntelligentAudio.NET\Models\NeuralHarmonic) om den saknas
            Directory.CreateDirectory(modelDir);
            _logger.LogInformation("[NeuralEngine] Created directory: {Path}", modelDir);
        }

        _logger.LogInformation("[NeuralEngine] Models missing. Starting download...");

        // Nu kan vi ladda ner utan DirectoryNotFoundException
        await DownloadEngineFilesAsync(ct);
    }

    private async ValueTask DownloadEngineFilesAsync(CancellationToken ct)
    {
        // URLs till dina hostade filer (t.ex. GitHub Releases, Azure eller din egen CDN)
        // MiniLM-L6-v2 är ca 80MB, vocab.txt ca 230KB.
        await DownloadFileAsync("https://huggingface.co/Xenova/all-MiniLM-L6-v2/resolve/main/onnx/model.onnx", _modelPath, ct);
        await DownloadFileAsync("https://huggingface.co/Xenova/all-MiniLM-L6-v2/resolve/main/vocab.txt", _vocabPath, ct);
    }

    private async ValueTask DownloadFileAsync(string url, string destination, CancellationToken ct)
    {
        // 1. Hämta bara headers först för att få filstorleken (Content-Length)
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        using var contentStream = await response.Content.ReadAsStreamAsync(ct);
        using var fileStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        // 2. Skapa en buffer för att strömma datan (Zero-allocation i loopen)
        byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);
        long totalRead = 0L;
        int read;
        int lastReportedPercent = -1;

        try
        {
            while ((read = await contentStream.ReadAsync(buffer, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read), ct);
                totalRead += read;

                // 3. Beräkna progress och logga var 10:e procent för att vara "clean"
                if (totalBytes != -1)
                {
                    int percent = (int)((totalRead * 100) / totalBytes);
                    if (percent % 10 == 0 && percent != lastReportedPercent)
                    {
                        _logger.LogInformation("[NeuralEngine] Downloading {FileName}: {Percent}%",
                            Path.GetFileName(destination), percent);
                        lastReportedPercent = percent;
                    }
                }
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        _logger.LogInformation("[NeuralEngine] Finished downloading: {FileName}", Path.GetFileName(destination));
    }


}