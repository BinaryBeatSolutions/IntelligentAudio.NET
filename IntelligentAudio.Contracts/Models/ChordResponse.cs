namespace IntelligentAudio.Contracts.Model;

public readonly record struct ChordResponse(
    Guid SessionId,
    int Note1,
    int Note2,
    int Note3,
    int Note4,
    long TimestampTicks // För att mäta latency i reklam-syfte!
);