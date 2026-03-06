namespace IntelligentAudio.API;


using Microsoft.Win32;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;

public class UdpOscReceiverService : BackgroundService
{
    private readonly InferenceRegistry _registry;
    private readonly ILogger<UdpOscReceiverService> _logger;
    private int _listenPort = 5001;

    // Injicera endast Registry-klassen, INTE dictionariet direkt
    public UdpOscReceiverService(InferenceRegistry registry, ILogger<UdpOscReceiverService> logger)
    {
        _registry = registry;
        _logger = logger;
        _listenPort = 5001;
    }


    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        using var udpClient = new UdpClient(_listenPort);
        _logger.LogInformation("[IntelligentAudio] OSC Receiver listening on port {Port}", _listenPort);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                // Vänta på inkommande OSC-svar från ljudmotorn
                var result = await udpClient.ReceiveAsync(ct);
                var span = result.Buffer.AsSpan();

                // 1. Läs OSC-adress (t.ex. "/ia/api/response")
                // Enkel parsnings-logik: OSC-strängar är null-terminerade till 4-byte boundary
                string address = ReadOscString(ref span);
                string tags = ReadOscString(ref span); // t.ex. ",siiii"
                
                _logger.LogInformation($"-------------------> {address} <--------------------");

                if (address == "/ia/api/response")
                {
                    // 2. Läs Session ID (sid)
                    string sidStr = ReadOscString(ref span);
                    if (Guid.TryParse(sidStr, out Guid sid))
                    {
                        // 3. Läs MIDI-noterna (4 stycken ints)
                        int[] notes = new int[4];
                        for (int i = 0; i < 4; i++)
                        {
                            notes[i] = ReadOscInt(ref span);
                        }

                        // 4. Mappa tillbaka till den väntande HTTP/SSE-förfrågan!
                        if (_registry.PendingRequests.TryRemove(sid, out var tcs))
                        {
                            tcs.TrySetResult(notes);
                            _logger.LogInformation("[OSC] Match found for SID: {Sid}", sid);
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error in OSC Receiver");
            }
        }
    }

    // OSC Hjälpmetoder (Hanterar 4-byte alignment)
    private string ReadOscString(ref Span<byte> data) { /* ... implementera padding ... */ return ""; }
    private int ReadOscInt(ref Span<byte> data) { /* ... läs BigEndian int ... */ return 0; }
}
