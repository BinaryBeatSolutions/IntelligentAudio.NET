using IntelligentAudio.Contracts.Events;
using IntelligentAudio.Contracts.Models;

var builder = Host.CreateApplicationBuilder(args);

// Registrera pipelinen som en Singleton (det finns bara ett rör i hela systemet)
builder.Services.AddSingleton<AudioPipeline>();
builder.Services.AddSingleton<IEventAggregator, DefaultEventAggregator>();
builder.Services.AddSingleton<IDawClientFactory, DefaultDawClientFactory>();
//builder.Services.AddSingleton<IIntelligentAudioService, AudioAnalysisService>();
builder.Services.AddHostedService<OscService>();
// Här kommer vi senare lägga till:
// builder.Services.AddHostedService<MicrophoneSource>();
// builder.Services.AddHostedService<AudioEngine>();

var host = builder.Build();
var eventAggregator = host.Services.GetRequiredService<IEventAggregator>();
var clientFactory = host.Services.GetRequiredService<IDawClientFactory>();


// Skapa en test-klient för Ableton på port 9001
//var testClientId = Guid.NewGuid();
//clientFactory.CreateClient(testClientId, 9001, "ableton");

//// Simulera att AI:n hittat ett ackord efter några sekunder
//_ = Task.Run(async () =>
//{
//    await Task.Delay(3000); //Detta generas på millisecs så du kanske inte hinner se detta meddelande under test. Öka då delay.
//    var testChord = new ChordInfo("Cmaj7", 0.99, DateTime.UtcNow);
    
//    // Publicerar eventet - detta ska trigga OscService!
//    eventAggregator.Publish(new ChordDetectedEvent(testClientId, testChord, DateTime.UtcNow));
//    Console.WriteLine($"{testChord.Name} {testClientId}");
//});
//// --- RÖKTEST SLUT ---

await host.RunAsync();
