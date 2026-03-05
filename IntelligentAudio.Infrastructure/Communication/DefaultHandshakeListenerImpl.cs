namespace IntelligentAudio.Infrastructure.Communication;

using BuildSoft.OscCore;
using IntelligentAudio.Contracts.Interfaces;
using IntelligentAudio.Contracts.Models;
using Microsoft.Extensions.Logging;
using System.Net;

public sealed partial class DefaultHandshakeListenerImpl : IHandshakeListener, IDisposable
{
    private readonly OscServer _server;
    private readonly OscClient _replyClient; // Denna saknades tidigare!
    private readonly IDawClientFactory _clientFactory;
    private readonly IClientManager _clientManager;
    private readonly ILogger<DefaultHandshakeListenerImpl> _logger;

    public DefaultHandshakeListenerImpl(
        IDawClientFactory clientFactory,
        IClientManager clientManager,
        ILogger<DefaultHandshakeListenerImpl> logger)
    {
        _clientFactory = clientFactory;
        _clientManager = clientManager;
        _logger = logger;

        // 1. Skapa servern för inkommande handskakning (Port 9005)
        _server = new OscServer(9005);

        // 2. Skapa en dedikerad klient för svar (Sänder till localhost 9005)
        // Vi sparar denna som en fältvariabel för att slippa 'new' i loopen
        _replyClient = new OscClient("127.0.0.1", 9005);

        // 3. Mappa metoden enligt din bild
        _server.TryAddMethod("/ia/handshake", OnHandshakeReceived);
    }

    public ValueTask StartListeningAsync(CancellationToken ct)
    {
        LogStarted(_logger, 9005);
        return ValueTask.CompletedTask;
    }

    private void OnHandshakeReceived(OscMessageValues values)
    {
        // Enligt din Max-bild: [0] int port, [1] string guid
        var preferredPort = values.ReadIntElement(0);
        var guidStr = values.ReadStringElement(1);

        if (Guid.TryParse(guidStr, out var clientId))
        {
            // Registrera klienten i systemet
            _clientFactory.CreateClient(clientId, preferredPort, DawType.Ableton);
            _clientManager.SetActiveClient(clientId);

            LogHandshakeSuccess(_logger, clientId, preferredPort);

            // 4. Skicka REPLY med vår dedikerade klient
            // Vi matchar din bilds adress: /ia/handshake/reply
            _replyClient.Send("/ia/handshake/reply", preferredPort);
        }
    }
    public void Dispose()
    {
        _server.Dispose();
        _replyClient.Dispose(); // Viktigt att stänga båda
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Handshake Listener active on port {Port}")]
    static partial void LogStarted(ILogger logger, int port);

    [LoggerMessage(Level = LogLevel.Information, Message = "Handshake OK: ID {Id} -> Port {Port}")]
    static partial void LogHandshakeSuccess(ILogger logger, Guid id, int port);
}
