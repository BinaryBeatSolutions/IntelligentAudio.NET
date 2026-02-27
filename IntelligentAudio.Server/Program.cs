

using IntelligentAudio.Contracts.Interfaces;
using IntelligentAudio.Infrastructure.Communication;

var builder = Host.CreateApplicationBuilder(args);

// Registrera pipelinen som en Singleton (det finns bara ett rör i hela systemet)
builder.Services.AddSingleton<AudioPipeline>();
builder.Services.AddSingleton<IEventAggregator, DefaultEventAggregator>();
builder.Services.AddSingleton<IClientFactory, DefaultClientFactory>();
builder.Services.AddSingleton<IIntelligentAudioService, AudioAnalysisService>();

// Här kommer vi senare lägga till:
// builder.Services.AddHostedService<MicrophoneSource>();
// builder.Services.AddHostedService<AudioEngine>();

var host = builder.Build();
await host.RunAsync();