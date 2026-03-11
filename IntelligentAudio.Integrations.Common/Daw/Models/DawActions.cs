

namespace IntelligentAudio.Integrations.Common.Daw.Models;


/// <summary>
/// Default events. 
/// </summary>
public enum DawAction : byte
{
    Play,
    Stop,
    Record,
    Undo,
    SetVolume,
    SelectTrack,
    SetLoop
}
