namespace IntelligentAudio.Integrations.Ableton;

using BuildSoft.OscCore;
using IntelligentAudio.Contracts.Interfaces;
using IntelligentAudio.Contracts.Models;
using IntelligentAudio.MusicTheory.Models;
using Microsoft.Extensions.Logging;

public sealed partial class AbletonDawClient : IDawClient
{
    private readonly OscClient _oscClient;
    private readonly ILogger<AbletonDawClient> _logger;

    // Cacheade OSC-adresser (Zero-Alloc)
    private static readonly string AddrPlay = "/live/play";
    private static readonly string AddrStop = "/live/stop";
    private static readonly string AddrRecord = "/live/record";

    public Guid ClientId { get; }
    public string Name => "Ableton Live";
    public int Port { get; }

    public AbletonDawClient(Guid clientId, int port, ILogger<AbletonDawClient> logger)
    {
        ClientId = clientId;
        Port = port;
        _logger = logger;
        _oscClient = new OscClient("127.0.0.1", port);
    }

    public ValueTask SendCommandAsync(DawCommand command)
    {
        // Snabb mappning utan nya sträng-allokeringar
        string? address = command.Action switch
        {
            DawAction.Play => AddrPlay,
            DawAction.Stop => AddrStop,
            DawAction.Record => AddrRecord,
            _ => null
        };

        if (address is not null)
        {
            // BuildSoft.OscCore skickar direkt utan boxing
            _oscClient.Send(address, 1);
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask SendChordAsync(ChordInfo chord)
    {
        // Skicka ackordnamn till din .amxd (matchar din bild)
        _oscClient.Send("/ia/chord/name", chord.Name);
        return ValueTask.CompletedTask;
    }

    public void Dispose() => _oscClient?.Dispose();
}
