using BuildSoft.OscCore;

namespace IntelligentAudio.Infrastructure.Communication;

public class OscService(
    IEventAggregator eventAggregator,
    IClientFactory clientFactory,
    ILogger<OscService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        // Vi prenumererar på kanalen för ChordDetectedEvent
        await foreach (var @event in eventAggregator.Subscribe<ChordDetectedEvent>(ct))
        {
            // 1. Hitta rätt "drivrutin" (Ableton, FL etc) via fabriken
            var client = clientFactory.GetClient(@event.ClientId);

            if (client is not null)
            {
                // 2. Skicka ackordet (nu med loggning och BuildSoft-prestanda!)
                await client.SendChordAsync(@event.Chord);
            }
            else
            {
                logger.LogWarning("Mottog ackord för okänd klient: {Id}", @event.ClientId);
            }
        }
    }
}