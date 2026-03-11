
namespace IntelligentAudio.Contracts.Interfaces;

public interface ICloudProvider
{
    /// <summary>
    /// Hämtar binär preset-data från molnet baserat på en nyckel.
    /// </summary>
    ValueTask<byte[]> DownloadPresetAsync(ParameterKey key);

    /// <summary>
    /// Kontrollerar om en nyare version av ett index finns i molnet.
    /// </summary>
    ValueTask<bool> CheckForUpdateAsync(ParameterKey key, int localVersion);
}