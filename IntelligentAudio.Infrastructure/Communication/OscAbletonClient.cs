using BuildSoft.OscCore;


namespace IntelligentAudio.Infrastructure.Communication;

public class OscAbletonClient(
    Guid clientId,
    string ip,
    int port,
    ILogger<OscAbletonClient> logger) : IDawClient
{
    private readonly OscClient _client = new(ip, port);

    public Guid ClientId => clientId;
    public string Name => "Ableton";
   // public int Port => port;

    public int Port { get; set; } = 0;

    public async Task SendChordAsync(ChordInfo chord)
    {
        if (chord is null) return;

        try
        {
            // Vi använder vår högpresterande extension-metod med uint-tags
            _client.SendChord("/ia/chord", chord.Name, (float)chord.Confidence);

            // Logga på Debug-nivå för att inte skräpa ner konsolen i produktion, 
            // men ge full insyn under utveckling.
            logger.LogDebug("[OSC OUT] Chord: {Name} (Conf: {Confidence:P0}) -> Port: {Port}",
                chord.Name, chord.Confidence, Port);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kunde inte skicka ackord till Ableton på port {Port}", Port);
        }

        await Task.CompletedTask;
    }

    public async Task SendCommandAsync(DawCommand command)
    {
        try
        {
            // Mappa och skicka kommando
            _client.SendTrigger($"/ia/control/{command.Action.ToString().ToLower()}", 1);

            logger.LogInformation("[OSC CMD] Executing: {Action} on Client: {Id}",
                command.Action, ClientId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Kommando-fel för DAW-klient {Id}", ClientId);
        }

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        logger.LogInformation("Stänger ner OSC-anslutning till {Name} (Port: {Port})", Name, Port);
        _client.Dispose();
    }
}
