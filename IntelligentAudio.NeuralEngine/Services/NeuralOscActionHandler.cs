
using BuildSoft.OscCore;
using IntelligentAudio.NeuralEngine.Abstractions;
using IntelligentAudio.Infrastructure.Extensions;
namespace IntelligentAudio.NeuralEngine.Services;

public sealed class NeuralOscActionHandler
{
    private readonly OscClient _client;

    public NeuralOscActionHandler(string ip = "127.0.0.1", int port = 11000)
    {
        // Latency Analysis: OscClient i OscCore är byggd för låg overhead
        _client = new OscClient(ip, port);
    }

    public void Execute(int parameterId, float value, bool isRelative)
    {
        // STRICT: Vi använder de mest direkta metoderna i OscCore
        if (isRelative)
        {
            // Ableton-bryggan (amxd) behöver stödja /relative
            // Exempel: "Raise cutoff" -> skickar +0.1
            _client.SendParameter("/live/device/set_parameter_relative", parameterId, value);
        }
        else
        {
            // Exempel: "Set cutoff to max" -> skickar 1.0
            _client.SendParameter("/live/device/set_parameter", parameterId, value);
        }
    }
}