



using IntelligentAudio.Contracts.Events;
using IntelligentAudio.Contracts.Models;

var builder = Host.CreateApplicationBuilder(args);

// Registrera pipelinen som en Singleton (det finns bara ett rör i hela systemet)
builder.Services.AddSingleton<AudioPipeline>();
builder.Services.AddSingleton<IEventAggregator, DefaultEventAggregator>();
builder.Services.AddSingleton<IDawClientFactory, DefaultDawClientFactory>();
//builder.Services.AddSingleton<IIntelligentAudioService, AudioAnalysisService>();

// Här kommer vi senare lägga till:
// builder.Services.AddHostedService<MicrophoneSource>();
// builder.Services.AddHostedService<AudioEngine>();

var host = builder.Build();


var eventAggregator = host.Services.GetRequiredService<IEventAggregator>();
var clientFactory = host.Services.GetRequiredService<IDawClientFactory>();

// Skapa en test-klient för Ableton på port 9001
var testClientId = Guid.NewGuid();
clientFactory.CreateClient(testClientId, 9001, "ableton");

// Simulera att AI:n hittat ett ackord efter 2 sekunder
_ = Task.Run(async () =>
{
    await Task.Delay(2000);
    var testChord = new ChordInfo("Cmaj7", 0.99, DateTime.UtcNow);

    // Publicerar eventet - detta ska trigga OscService!
    eventAggregator.Publish(new ChordDetectedEvent(testClientId, testChord, DateTime.UtcNow));
});
// --- RÖKTEST SLUT ---

await host.RunAsync();
