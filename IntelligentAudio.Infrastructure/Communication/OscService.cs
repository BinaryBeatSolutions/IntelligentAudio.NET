using BuildSoft.OscCore;
using IntelligentAudio.Contracts.Models;

namespace IntelligentAudio.Infrastructure.Communication;

/// <summary>
/// Network communication
/// </summary>
/// <param name="eventAggregator"></param>
/// <param name="clientFactory"></param>
/// <param name="logger"></param>
public class OscService(
    //IEventAggregator eventAggregator,
    IEnumerable<IParameterDiscoveryHandler> discoveryHandlers,
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
                    int serverSendPort = 0;
                    using (var tempSocket = new UdpClient(0))
                    {
                        serverSendPort = ((IPEndPoint)tempSocket.Client.LocalEndPoint).Port;
                    }
                 
                    var client = clientFactory.CreateClient(clientId, serverSendPort, DawType.Ableton);
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

        _receiver.TryAddMethod("/ia/device/parameters", message =>
        {
            // 1. Signalera start
            foreach (var handler in discoveryHandlers) handler.OnDiscoveryStarted();

            // 2. Parsa paketet (Zero-allocation)
            for (int i = 0; i < message.ElementCount; i += 2)
            {
                int id = message.ReadIntElement(i);
                string name = message.ReadStringElement(i + 1);

                // Skicka vidare till AI-motorn som Spans
                foreach (var handler in discoveryHandlers)
                {
                    handler.OnParameterDiscovered(id, name.AsMemory().Span);
                }
            }

            // 3. Signalera klart
            foreach (var handler in discoveryHandlers) handler.OnDiscoveryCompleted();
        });
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
