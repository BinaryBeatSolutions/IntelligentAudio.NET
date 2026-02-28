using BuildSoft.OscCore;
using Newtonsoft.Json;
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
        logger.LogInformation("OSC Discovery Server startad på port {Port}", DiscoveryPort);

        // 2. Registrera Handshake-metoden
        _receiver.TryAddMethod("/ia/handshake", message =>
        {
            try
            {
                // 1. Läs det FÖRSTA elementet (index 0) som ett heltal (Porten)
                int clientPort = message.ReadIntElement(0);

                // 2. Läs det ANDRA elementet (index 1) som en sträng (GUID)
                string guidStr = message.ReadStringElement(1);

                if (Guid.TryParse(guidStr, out var clientId))
                {
                    // Validera porten (måste vara mellan 1024 och 65535)
                    if (clientPort >= 1024 && clientPort <= 65535)
                    {
                        clientFactory.CreateClient(clientId, clientPort, "ableton");
                        logger.LogInformation("Handshake lyckades! Klient {Id} på port {Port}",
                            clientId, clientPort);
                    }
                    else
                    {
                        logger.LogWarning("Mottog ogiltig port: {Port}", clientPort);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Fel vid läsning av OSC-handshake (Ordning: Int, String)");
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


    private void ParseMessages(OscMessageValues message)
    {


        // Logga för att se exakt vad som händer i konsolen
        logger.LogInformation("Mottog OSC-meddelande. Antal element: {Count}", message.ElementCount);

        if (message.ElementCount < 2)
        {
            logger.LogWarning("Meddelandet saknar argument (GUID och Port).");
            return;
        }

        try
        {

            // 1. Läs GUID (Index 0)
            string guidStr = message.ReadStringElement(0);

            // 2. Försök läsa som Float först (Max skickar ofta floats även om det ser ut som int)
            float portAsFloat = message.ReadFloatElement(1);
            int clientPort = Convert.ToInt32(portAsFloat);

            // 3. Om porten fortfarande är orimlig, tvinga den till 9005
            if (clientPort <= 0 || clientPort > 65535) clientPort = 9005;


            //string guidStr = message.ReadStringElement(0);
            //// Använd ReadInt eller ReadFloat beroende på vad Ableton faktiskt skickar
            //var clientPort = (int)Convert.ToInt32(message.ReadStringElement(1)) / 10;

            if (Guid.TryParse(guidStr, out var clientId))
            {
                clientFactory.CreateClient(clientId, clientPort, "ableton");

                logger.LogInformation("Handshake lyckades för {Id}", clientId);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Krasch vid läsning av element.");
        }
    }


    public override void Dispose()
    {
        _receiver?.Dispose();
        base.Dispose();
    }
}
