
namespace IntelligentAudio.Contracts.Models;

/// <summary>
/// Default events. 
/// </summary>
public enum DawAction
{
    // Transport
    Play,
    Stop,
    Pause,
    Record,
    ToggleLoop,

    // Navigation & Editing
    Undo,
    Redo,
    NextTrack,
    PreviousTrack,

    // Track Management
    ArmTrack,
    SoloTrack,
    MuteTrack,

    // Session Specific
    AddMidiClip,
    FireScene,

    // Global
    SetTempo,
    SaveProject
}
