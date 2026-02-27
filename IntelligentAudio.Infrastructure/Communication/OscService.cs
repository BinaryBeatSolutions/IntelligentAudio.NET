using BuildSoft.OscCore;

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
        logger.LogInformation("ExecuteAsync");

        // 1. Starta servern direkt
        _receiver = new OscServer(DiscoveryPort);

        // 2. Registrera handskakningen
        _receiver.TryAddMethod("/ia/handshake", (message) =>
        {
            string guid = message.ReadStringElement(0);
            int port = message.ReadIntElement(1);
            clientFactory.CreateClient(Guid.Parse(guid), port, "ableton");
            logger.LogInformation("Handshake Success: {Id}", guid);
        });

        // 3. Håll tjänsten vid liv (viktigt i .NET 10 BackgroundService)
        await Task.Delay(-1, ct);
    }

    //protected override async Task ExecuteAsync(CancellationToken ct)
    //{
    //    // 1. Starta mottagaren på standardporten
    //    _receiver = new OscServer(DiscoveryPort);
    //    logger.LogInformation("OSC Discovery Server startad på port {Port}", DiscoveryPort);

    //    // 2. Registrera Handshake-metoden
    //    _receiver.TryAddMethod("/ia/handshake", message =>
    //    {
    //        try
    //        {
    //            // Korrekt BuildSoft syntax för att läsa element
    //            string guidStr = message.ReadStringElement(0);
    //            int clientPort = message.ReadIntElement(1);

    //            if (Guid.TryParse(guidStr, out var clientId))
    //            {
    //                // Registrera klienten i fabriken (den skapar en ny OscAbletonClient om den inte finns)
    //                clientFactory.CreateClient(clientId, clientPort, "ableton");

    //                logger.LogInformation("Handshake mottaget: Klient {Id} lyssnar nu på port {Port}",
    //                    clientId, clientPort);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.LogError(ex, "Fel vid parsning av handshake-meddelande.");
    //        }
    //    });

    //    // 3. Huvudloopen: Lyssna på interna events (Ackord från AI:n)
    //    await foreach (var @event in eventAggregator.Subscribe<ChordDetectedEvent>(ct))
    //    {
    //        var client = clientFactory.GetClient(@event.ClientId);
    //        if (client is not null)
    //        {
    //            await client.SendChordAsync(@event.Chord);
    //        }
    //    }
    //}

    public override void Dispose()
    {
        _receiver?.Dispose();
        base.Dispose();
    }
}
