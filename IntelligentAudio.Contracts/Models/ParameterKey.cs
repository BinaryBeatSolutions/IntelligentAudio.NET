
namespace IntelligentAudio.Contracts.Models;


/// <summary>
/// Bit-packad 64-bitars nyckel för blixtsnabb indexering.
/// Layout: [4 bit Namespace | 12 bit Provider | 24 bit Entity | 24 bit SubParam/Version]
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct ParameterKey(ulong Value)
{
    public byte Namespace => (byte)(Value >> 60);
    public ushort Provider => (ushort)((Value >> 48) & 0xFFF);
    public uint Entity => (uint)((Value >> 24) & 0xFFFFFF);
    public uint SubParam => (uint)(Value & 0xFFFFFF);

    public static ParameterKey Create(byte ns, ushort prov, uint ent, uint sub)
        => new((ulong)(ns & 0xF) << 60 | (ulong)(prov & 0xFFF) << 48 | (ulong)(ent & 0xFFFFFF) << 24 | (sub & 0xFFFFFF));
}

[StructLayout(LayoutKind.Explicit, Size = 24)]
public readonly struct RegistryEntry
{
    [FieldOffset(0)] public readonly ParameterKey Key;
    [FieldOffset(8)] public readonly long Offset;  // 8 bytes
    [FieldOffset(16)] public readonly long Length; // 8 bytes (Totalt 24)

    public RegistryEntry(ParameterKey key, long offset, long length)
    {
        Key = key;
        Offset = offset;
        Length = length;
    }
}

/// <summary>
/// Representerar ett segment i din Memory-Mapped File eller Cloud Blob.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record struct NexusResource(long Offset, long Length, bool IsLocal);