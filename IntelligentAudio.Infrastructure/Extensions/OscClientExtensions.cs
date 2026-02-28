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