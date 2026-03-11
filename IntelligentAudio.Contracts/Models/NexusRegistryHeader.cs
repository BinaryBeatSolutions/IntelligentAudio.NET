namespace IntelligentAudio.Contracts.Models;

/// <summary>
/// Den första delen av din MMF-fil. Berättar för Nexus hur den ska läsa resten.
/// </summary>
[StructLayout(LayoutKind.Explicit, Size = 64)]
public readonly struct NexusRegistryHeader
{
    [FieldOffset(0)] public readonly ulong MagicBytes;    // "IANEXUS!"
    [FieldOffset(8)] public readonly int Version;
    [FieldOffset(12)] public readonly long EntryCount;
    [FieldOffset(20)] public readonly long IndexOffset;
    [FieldOffset(28)] public readonly long DataOffset;

    // Konstruktor för att kunna skapa headern i NexusStorageEngine (Infrastructure)
    public NexusRegistryHeader(ulong magicBytes, int version, long entryCount, long indexOffset, long dataOffset)
    {
        MagicBytes = magicBytes;
        Version = version;
        EntryCount = entryCount;
        IndexOffset = indexOffset;
        DataOffset = dataOffset;
    }
}