


var builder = Host.CreateApplicationBuilder(args);

// Registrera pipelinen som en Singleton (det finns bara ett rör i hela systemet)
builder.Services.AddSingleton<AudioPipeline>();

// Här kommer vi senare lägga till:
// builder.Services.AddHostedService<MicrophoneSource>();
// builder.Services.AddHostedService<AudioEngine>();

var host = builder.Build();
await host.RunAsync();