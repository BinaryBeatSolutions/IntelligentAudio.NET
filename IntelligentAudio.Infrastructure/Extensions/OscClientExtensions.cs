using BuildSoft.OscCore;

namespace IntelligentAudio.Infrastructure.Extensions;

public static class OscClientExtensions
{
    // Tag-hash för en sträng och en float (",sf")
    // I OscCore motsvarar detta uint-värdet 6649484u (',' + 's' + 'f')
    private const uint StringFloatTags = 6649484u;

    // Tag-hash för en enskild integer (",i") - används för triggers/commands
    private const uint IntTag = 28160u;

    public static void SendChord(this OscClient client, string address, string name, float confidence)
    {
        var writer = client.Writer;

        // 1. Skriv adressen och de förkompilerade taggarna
        writer.WriteAddressAndTags(address, StringFloatTags);

        // 2. Skriv datan direkt i bufferten (Zero-allocation)
        writer.Write(name);
        writer.Write(confidence);

        // 3. Skicka direkt via den interna socketen
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