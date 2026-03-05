



using IntelligentAudio.Infrastructure.Controllers;
using IntelligentAudio.Infrastructure.Services;
using IntelligentAudio.MusicTheory;
using IntelligentAudio.Server.Handlers.Music;

var builder = Host.CreateApplicationBuilder(args);

// Register the pipeline as a Singleton (there is only one pipe in the entire system)
builder.Services.AddSingleton<AudioPipeline>();
builder.Services.AddSingleton<IEventAggregator, DefaultEventAggregator>();
builder.Services.AddSingleton<IDawClientFactory, DefaultDawClientFactory>();
builder.Services.AddSingleton<SimpleHighPassFilter>();
builder.Services.AddSingleton(new NoiseGateProcessor { Threshold = 0.012f }); // (400 / 32768 ≈ 0.0122)
builder.Services.AddHostedService<OscService>();
builder.Services.AddSingleton<IAudioBufferProvider, DefaultAudioBufferProviderImpl>();
builder.Services.AddSingleton<DefaultWhisperModelService>();
builder.Services.AddHostedService<WhisperInferenceWorker>();
builder.Services.AddSingleton<ChordFactory>();
builder.Services.AddTransient<IIntentHandler, MusicTheoryHandler>();
builder.Services.AddSingleton<IDawClientFactory, DefaultDawClientFactory>();

builder.Services.AddSingleton<IClientManager, DefaultClientManagerImpl>();
builder.Services.AddSingleton<IDawCommandController, DefaultDawCommandControllerImpl>();
builder.Services.AddSingleton<IHandshakeListener, DefaultHandshakeListenerImpl>();

//Windows and MacOS so far.
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)){builder.Services.AddSingleton<IAudioStreamSource, WindowsAudioSource>();}
else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)){ builder.Services.AddSingleton<IAudioStreamSource, MacAudioSource>(); }

builder.Services.AddHostedService<MicrophoneSource>();

var host = builder.Build();

// Ensure model is loaded
var modelService = host.Services.GetRequiredService<DefaultWhisperModelService>();
await modelService.EnsureModelReadyAsync(WhisperModelType.Tiny, CancellationToken.None);

var pipeline = host.Services.GetRequiredService<AudioPipeline>();
var eventAggregator = host.Services.GetRequiredService<IEventAggregator>();
var clientFactory = host.Services.GetRequiredService<IDawClientFactory>();
var audioBuffer = host.Services.GetRequiredService<IAudioBufferProvider>();
var audioSource = host.Services.GetRequiredService<IAudioStreamSource>();

await host.RunAsync();
