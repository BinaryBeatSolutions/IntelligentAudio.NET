using BuildSoft.OscCore;

namespace IntelligentAudio.Infrastructure.Communication;

/// <summary>
/// Network communication
/// </summary>
/// <param name="eventAggregator"></param>
/// <param name="clientFactory"></param>
/// <param name="logger"></param>
public class OscService(
    IEventAggregator eventAggregator,
    IDawClientFactory clientFactory,
    ILogger<OscService> logger) : BackgroundService
{
    private OscServer? _receiver;
    private const int DiscoveryPort = 9000; // Server port, move to json in future

    /// <summary>
    /// Starts communication
    /// </summary>
    /// <param name="ct">CancellationToken ct</param>
    /// <returns></returns>
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Starta mottagaren på standardporten
        _receiver = new OscServer(DiscoveryPort);
        logger.LogInformation("[IntelligentAudio.NET] OSC Discovery Server started on port {Port}", DiscoveryPort);

        // Register Handshake
        _receiver.TryAddMethod("/ia/handshake", message =>
        {
            try
            {
                // UDP in ableton, needs to send the port first in packet
                // Ableton sends: /ia/handshake [port] [guid]
                int clientListenPort = message.ReadIntElement(0); // Porten Ableton lyssnar på
                string guidStr = message.ReadStringElement(1);

                if (Guid.TryParse(guidStr, out var clientId))
                {
                    // --- STRATEGY FOR DYNAMIC PORTS ---
                    int serverSendPort;
                    using (var tempSocket = new UdpClient(0))
                    {
                        serverSendPort = ((IPEndPoint)tempSocket.Client.LocalEndPoint).Port;
                    }
                 
                    var client = clientFactory.CreateClient(clientId, serverSendPort, "ableton");
                    var sender = new OscClient("127.0.0.1", serverSendPort);
                    sender.Send("/ia/handshake/reply", serverSendPort);

                    logger.LogInformation("[IntelligentAudio.NET] Handshake OK! Client {Id}. Server reply with port {serverSendPort}", clientId, serverSendPort); 
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Error] Handshake failed.");
            }
        });

        //Main loop
        await foreach (var @event in eventAggregator.Subscribe<ChordDetectedEvent>(ct))
        {
            /*
             * Her we can add serveral types, like send play, stop etc.
             */
            var client = clientFactory.GetClient(@event.ClientId);
            if (client is not null)
            {
                await client.SendChordAsync(@event.Chord);
            }
        }
    }

    /// <summary>
    /// Cleanup
    /// </summary>
    public override void Dispose()
    {
        _receiver?.Dispose();
        base.Dispose();
    }
}
