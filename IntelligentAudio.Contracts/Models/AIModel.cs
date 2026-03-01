
namespace IntelligentAudio.Contracts.Models;

public class AIModel
{
    public byte[] AudioData { get; set; }

    // Eventuellt metadata
    public string Name { get; set; }
    public TimeSpan Duration { get; set; }

    public AIModel(byte[] data)
    {
        AudioData = data;
    }
}