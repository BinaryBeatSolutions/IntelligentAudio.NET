using BuildSoft.OscCore;
using Newtonsoft.Json;
using System.Net;
using System.Text.Json.Serialization;

namespace IntelligentAudio.Infrastructure.Communication;

public class OscService(
    IEventAggregator eventAggregator,
    IDawClientFactory clientFactory,
    ILogger<OscService> logger) : BackgroundService
{
    private OscServer? _receiver;
    private const int DiscoveryPort = 9000; // Serverns fasta "postlåda"


    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // 1. Starta mottagaren på standardporten
        _receiver = new OscServer(DiscoveryPort);
        logger.LogInformation("[IntelligentAudio.NET] OSC Discovery Server startad på port {Port}", DiscoveryPort);

        // 2. Registrera Handshake-metoden
        _receiver.TryAddMethod("/ia/handshake", message =>
        {
            try
            {
                // Ableton skickar: /ia/handshake [port] [guid]
                int clientListenPort = message.ReadIntElement(0); // Porten Ableton lyssnar på
                string guidStr = message.ReadStringElement(1);

                if (Guid.TryParse(guidStr, out var clientId))
                {
                    // --- STRATEGI FÖR DYNAMISK PORT ---

                    // 1. Skapa en temporär UDP-klient för att hitta en ledig port på servern
                    int serverSendPort;
                    using (var tempSocket = new System.Net.Sockets.UdpClient(0))
                    {
                        serverSendPort = ((System.Net.IPEndPoint)tempSocket.Client.LocalEndPoint).Port;
                    }

                    // 2. Registrera klienten i din factory med den NYA serverporten
                    // (Du kan behöva uppdatera CreateClient så den sparar 'serverSendPort')
                    var client = clientFactory.CreateClient(clientId, serverSendPort, "ableton");

                    // 3. SKICKA TILLBAKA SVAR TILL ABLETON
                    // Vi skickar tillbaka den dynamiska porten till Ableton på deras lyssningsport
                    var sender = new OscClient("127.0.0.1", serverSendPort);
                    sender.Send("/ia/handshake/reply", serverSendPort);

                    logger.LogInformation("Handshake OK! Klient {Id}. Servern skickar via port {serverSendPort}", clientId, serverSendPort);

                    
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fel vid handshake");
            }
        });



        // 3. Huvudloopen: Lyssna på interna events (Ackord från AI:n)
        await foreach (var @event in eventAggregator.Subscribe<ChordDetectedEvent>(ct))
        {
            var client = clientFactory.GetClient(@event.ClientId);
            if (client is not null)
            {
                await client.SendChordAsync(@event.Chord);
            }
        }
    }

    public override void Dispose()
    {
        _receiver?.Dispose();
        base.Dispose();
    }
}
