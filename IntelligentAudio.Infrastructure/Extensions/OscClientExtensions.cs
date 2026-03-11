using BuildSoft.OscCore;

namespace IntelligentAudio.Infrastructure.Extensions;

/// <summary>
/// Extensions for OscService
/// </summary>
public static class OscClientExtensions
{
    // Tag hash for a string and a float (",sf")
    // In OscCore this corresponds to the uint value 6649484u (',' + 's' + 'f')
    private const uint StringFloatTags = 6649484u;

    // Tag hash for a single integer (",i") - used for triggers/commands
    private const uint IntTag = 28160u;


    // NY TAG: Hash för en integer och en float (",if")
    // Beräknat värde för ',' + 'i' + 'f' i OscCore-format
    private const uint IntFloatTags = 6633644u;


    /// <summary>
    /// Sends a parameter change to Ableton Live with Zero-Allocation.
    /// </summary>
    public static void SendParameter(this OscClient client, string address, int parameterId, float value)
    {
        var writer = client.Writer;

        // 1. Skriv adressen och ",if"-taggen (blixtsnabbt via din hash-logik)
        writer.WriteAddressAndTags(address, IntFloatTags);

        // 2. Skriv ID och Värde direkt i bufferten
        writer.Write(parameterId);
        writer.Write(value);

        // 3. Skicka direkt via socketen - noll kopiering, noll heap-tryck
        client.Socket.Send(writer.Buffer, writer.Length, System.Net.Sockets.SocketFlags.None);
    }

    public static void SendChord(this OscClient client, string address, string name, float confidence)
    {
        var writer = client.Writer;

        // 1. Write the address and the precompiled tags
        writer.WriteAddressAndTags(address, StringFloatTags);

        // 2. Write the data directly into the buffer (Zero-allocation)
        writer.Write(name);
        writer.Write(confidence);

        // 3. Send directly via the internal socket
        client.Socket.Send(writer.Buffer, writer.Length, System.Net.Sockets.SocketFlags.None);
    }

    public static void SendTrigger(this OscClient client, string address, int value)
    {
        var writer = client.Writer;
        writer.WriteAddressAndTags(address, IntTag);
        writer.Write(value);
        client.Socket.Send(writer.Buffer, writer.Length, System.Net.Sockets.SocketFlags.None);
    }
}