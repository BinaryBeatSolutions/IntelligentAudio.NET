
var builder = Host.CreateApplicationBuilder(args);

// Register the pipeline as a Singleton (there is only one pipe in the entire system)
builder.Services.AddSingleton<AudioPipeline>();
builder.Services.AddSingleton<IEventAggregator, DefaultEventAggregator>();
builder.Services.AddSingleton<IDawClientFactory, DefaultDawClientFactory>();
//builder.Services.AddSingleton<IIntelligentAudioService, AudioAnalysisService>();
builder.Services.AddHostedService<OscService>();

// Will add these later:
// builder.Services.AddHostedService<MicrophoneSource>();
// builder.Services.AddHostedService<AudioEngine>();

var host = builder.Build();

var eventAggregator = host.Services.GetRequiredService<IEventAggregator>();
var clientFactory = host.Services.GetRequiredService<IDawClientFactory>();

// Create a client Ableton on port 9001
// var testClientId = Guid.NewGuid();
// clientFactory.CreateClient(testClientId, 9001, "ableton")
// Simulate a callback with a chord.
//_ = Task.Run(async () =>
//{
//    await Task.Delay(3000); //Detta generas på millisecs så du kanske inte hinner se detta meddelande under test. Öka då delay.
//    var testChord = new ChordInfo("Cmaj7", 0.99, DateTime.UtcNow);
//    eventAggregator.Publish(new ChordDetectedEvent(testClientId, testChord, DateTime.UtcNow));
//    Console.WriteLine($"{testChord.Name} {testClientId}");
//});
// --- SMOKE TEST ---

await host.RunAsync();
