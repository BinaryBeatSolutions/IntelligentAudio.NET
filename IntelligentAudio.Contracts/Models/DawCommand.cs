namespace IntelligentAudio.Contracts.Models;

/// <summary>
/// Representerar ett generiskt kommando som skickas till en DAW.
/// </summary>
/// <param name="Action">Vilken typ av åtgärd (Play, Record, etc.)</param>
/// <param name="TargetIndex">Valfritt index för spår, scen eller klipp (t.ex. Track 3)</param>
/// <param name="Value">Valfritt värde (t.ex. BPM-tal eller volymnivå)</param>
public record DawCommand(
    DawAction Action,
    int? TargetIndex = null,
    object? Value = null);
