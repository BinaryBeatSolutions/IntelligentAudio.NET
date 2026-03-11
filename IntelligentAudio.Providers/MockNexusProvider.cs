
namespace IntelligentAudio.Providers;

public class MockNexusProvider : INexusProvider
{
    public ValueTask<NexusResource> ResolveResourceAsync(ParameterKey key)
    {
        // Vi simulerar en snabb träff i vårt "index"
        return ValueTask.FromResult(new NexusResource
        {
            IsLocal = true,
            Offset = 42,
            Length = 128
        });
    }

    public ValueTask<ReadOnlyMemory<byte>> GetParameterDataAsync(ParameterKey key)
    {
        // Returnera lite demo-data så att Dashboarden faktiskt kan visa något
        var demoData = Encoding.UTF8.GetBytes($"[DEMO MODE] Data for Key: {key.Value:X}");
        return ValueTask.FromResult(new ReadOnlyMemory<byte>(demoData));
    }
}