using BuildSoft.OscCore;

namespace IntelligentAudio.Infrastructure.Communication;

public class OscAbletonClient : IDawClient
{
    private readonly OscClient _client;
    public Guid ClientId { get; }
    public string Name => "Ableton Live (BuildSoft Engine)";
    public int Port { get; }

    public OscAbletonClient(Guid clientId, string ip, int port)
    {
        ClientId = clientId;
        Port = port;

        // I BuildSoft skapar vi en klient direkt mot IP och Port.
        // Den sköter UDP-hanteringen internt på ett högpresterande sätt.
        _client = new OscClient(ip, port);
    }

    public async Task SendChordAsync(ChordInfo chord)
    {
        // BuildSoft använder en effektiv Send-metod som stöder multipla argument
        _client.Send("/ia/chord", chord.Name, (float)chord.Confidence);

        await Task.CompletedTask;
    }

    public async Task SendCommandAsync(DawCommand command)
    {
        // Mappa din DawAction till Abletons specifika OSC-sökvägar
        string path = command.Action switch
        {
            DawAction.Play => "/live/remote/transport/play",
            DawAction.Stop => "/live/remote/transport/stop",
            DawAction.Record => "/live/remote/transport/record",
            _ => $"/ia/control/{command.Action.ToString().ToLower()}"
        };

        // Skicka värdet 1 för att trigga händelsen i Max for Live [udpreceive]
        _client.Send(path, 1);

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        // BuildSoft-klienten stänger sina resurser här
        _client.Dispose();
    }
}
