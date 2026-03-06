using System.Collections.Concurrent;

namespace IntelligentAudio.API;

public class InferenceRegistry
{
    // Håller koll på TaskCompletionSource för varje SID
    public ConcurrentDictionary<Guid, TaskCompletionSource<int[]>> PendingRequests { get; } = new();
}