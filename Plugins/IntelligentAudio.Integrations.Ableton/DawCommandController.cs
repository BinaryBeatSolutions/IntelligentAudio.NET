
using BuildSoft.OscCore;

namespace IntelligentAudio.Integrations.Ableton;

public class AbletonOscController : IDawController
{
    private readonly OscClient _client;

    public AbletonOscController(string ip, int port)
    {
        // Här skapas den faktiska OSC-klienten från biblioteket
        _client = new OscClient(ip, port);
    }

    public async Task ExecuteAsync(DawCommand command, CancellationToken ct)
    {
        var address = command.Action switch
        {
            DawAction.Play => "/live/transport/start",
            DawAction.Stop => "/live/transport/stop",
            DawAction.Record => "/live/transport/record",
            _ => null
        };

        if (address != null)
        {
            // Specifikt anrop för OscCore
            _client.Send(address, 1);
        }

        await Task.CompletedTask;
    }
}