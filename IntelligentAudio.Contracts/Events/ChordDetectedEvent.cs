
namespace IntelligentAudio.Contracts.Events;

public record ChordDetectedEvent(
    Guid ClientId,
    ChordInfo Chord,
    DateTime Timestamp);
