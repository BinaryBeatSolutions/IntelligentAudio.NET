using BuildSoft.OscCore;

namespace IntelligentAudio.Infrastructure.Communication;

public class OscService(
    IEventAggregator eventAggregator,
    IClientFactory clientFactory,
    ILogger<OscService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("OSC Service startad. Lyssnar efter ackord...");

        // Vi prenumererar på postkontoret. 
        // ReadAllAsync i .NET 10 är extremt effektivt för detta.
        await foreach (var @event in eventAggregator.Subscribe<ChordDetectedEvent>(ct))
        {
            try
            {
                // Hämta rätt klient (t.ex. baserat på porten i appsettings.json)
                var client = clientFactory.GetClient(@event.ClientId);

                if (client != null)
                {
                    // Skicka ackordet till Ableton via OscCore
                    await client.SendChordAsync(@event.Chord);

                    logger.LogDebug("OSC skickat: {Chord} till port {Port}", @event.Chord.Name, client.Port);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Kunde inte skicka OSC-meddelande");
            }
        }
    }
}