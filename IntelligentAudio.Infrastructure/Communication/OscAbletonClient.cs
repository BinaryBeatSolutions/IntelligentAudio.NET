using BuildSoft.OscCore;

namespace IntelligentAudio.Infrastructure.Communication;

/// <summary>
/// Ableton client
/// </summary>
/// <param name="clientId"></param>
/// <param name="ip"></param>
/// <param name="port"></param>
/// <param name="logger"></param>
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

    /// <summary>
    /// Send Chord to DAW
    /// </summary>
    /// <param name="chord"></param>
    /// <returns></returns>
    public async Task SendChordAsync(ChordInfo chord)
    {
        if (chord is null) return;

        try
        {
            
            _client.SendChord("/ia/chord", chord.Name, (float)chord.Confidence);

            logger.LogDebug("[IntelligentAudio.NET] Chord: {Name} (Conf: {Confidence:P0}) -> Port: {Port}",
                chord.Name, chord.Confidence, Port);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Error] Couldn't send command to Ableton on port {Port}", Port);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Send any command to DAW. 
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public async Task SendCommandAsync(DawCommand command)
    {
        try
        {
            // Map and send command
            _client.SendTrigger($"/ia/control/{command.Action.ToString().ToLower()}", 1);

            logger.LogInformation("[IntelligentAudio.NET] Executing: {Action} on Client: {Id}", command.Action, ClientId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Error] Command failed. ID: {Id}", ClientId);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    public void Dispose()
    {
        logger.LogInformation("Stänger ner OSC-anslutning till {Name} (Port: {Port})", Name, Port);
        _client.Dispose();
    }
}
