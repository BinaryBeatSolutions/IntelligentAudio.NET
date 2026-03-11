namespace IntelligentAudio.Infrastructure.Communication;

public class VstBridgeClient(Guid clientId, int port) : IDawClient
{
    public Guid ClientId => clientId;
    public string Name => "Generic VST Bridge";
    
    public int Port => port;

    public async ValueTask SendChordAsync(ChordInfo chord)
    {
        // Prata med VST-instansen via Localhost
        await Task.CompletedTask;
    }

    public async ValueTask SendCommandAsync(DawCommand command)
    {
        await Task.CompletedTask;
    }

    public void Dispose() { }
}