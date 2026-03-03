

using BuildSoft.OscCore;
using IntelligentAudio.MusicTheory.Models;

namespace IntelligentAudio.Integrations.Ableton;

public sealed class AbletonDawClient : IIntentReceiver, IDawClient
{
    private readonly OscClient _oscClient;
    private readonly ILogger<AbletonDawClient> _logger;

    public Guid Id { get; } = Guid.NewGuid();
    public string Name => "Ableton Live";
    public int Port { get; }
    public bool IsConnected => _oscClient != null;

    public Guid ClientId => throw new NotImplementedException();

    public AbletonDawClient(string ip, int port, ILogger<AbletonDawClient> logger)
    {
        Port = port;
        _logger = logger;
        // Här skapas den faktiska OSC-anslutningen (BuildSoft.OscCore)
        _oscClient = new OscClient(ip, port);
    }

    /// <summary>
    /// Den agnostiska ingången för motorn. 
    /// Motorn skickar ett "paket" (T), och denna metod packar upp det.
    /// </summary>
    public async Task ReceiveAsync<T>(T intent, CancellationToken ct) where T : class
    {
        if (intent is DawCommand command)
        {
            await SendCommandAsync(command);
        }
        else if (intent is ChordInfo chord)
        {
            await SendChordAsync(chord);
        }
        else
        {
            _logger.LogWarning("[AbletonClient] Unknown intent type: {type}", typeof(T).Name);
        }
    }

    public async Task SendCommandAsync(DawCommand command)
    {
        // Mappa DawAction till specifika OSC-adresser i Ableton
        var address = command.Action switch
        {
            DawAction.Play => "/live/play",
            DawAction.Stop => "/live/stop",
            DawAction.Record => "/live/record",
            _ => null
        };

        if (address != null)
        {
            _logger.LogDebug("[OSC] Sending {addr} to Ableton on port {port}", address, Port);
            _oscClient.Send(address, 1);
        }
        await Task.CompletedTask;
    }

    public async Task SendChordAsync(ChordInfo chord)
    {
        // Här kan vi skicka ackordnamnet till en text-display i din .amxd
        _oscClient.Send("/ia/chord/name", chord.Name);

        // Eller skicka råa MIDI-noter om din amxd förväntar sig det
        // _oscClient.Send("/ia/chord/notes", chord.MidiNotes);

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        _oscClient?.Dispose();
    }
}
