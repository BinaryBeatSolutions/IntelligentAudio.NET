

namespace IntelligentAudio.Engine.Services;

public class ModelService : IHostedService
{
    private readonly IEventAggregator _eventAggregator;
    private readonly ILogger _logger;

    public ModelService(
        IEventAggregator eventAggregator,
        ILogger<ModelService> logger) 
    {
        _logger = logger;
        _eventAggregator = eventAggregator;
    }

    public Task StartAsync(CancellationToken ct)
    {
        _logger.LogInformation("[IntelligentAudio.NET] Starting model service…");

        // Trigger download if model doesn't exist as soon as the service starts
        //_eventAggregator.Publish(new AIModel());
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}