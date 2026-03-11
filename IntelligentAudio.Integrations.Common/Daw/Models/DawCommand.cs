

namespace IntelligentAudio.Integrations.Common.Daw.Models;

/// <summary>
/// Representerar ett generiskt kommando som skickas till en DAW.
/// </summary>
/// <param name="Action">Vilken typ av åtgärd (Play, Record, etc.)</param>
/// <param name="TargetIndex">Valfritt index för spår, scen eller klipp (t.ex. Track 3)</param>
/// <param name="Value">Valfritt värde (t.ex. BPM-tal eller volymnivå)</param>
public readonly record struct DawCommand(
    DawAction Action,
    int TargetIndex = -1, // -1 istället för null för att undvika Nullable-overhead
    float Value = 0f      // Float täcker volym, pan, tempo utan boxing
);
