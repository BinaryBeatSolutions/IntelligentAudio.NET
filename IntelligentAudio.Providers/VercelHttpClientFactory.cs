
namespace IntelligentAudio.Providers;

public static class VercelHttpClientFactory
{
    public static HttpClient Create()
    {
        // SocketsHttpHandler är motorn i .NET 10 för rå nätverksprestanda
        var handler = new SocketsHttpHandler
        {
            // Tillåt flera strömmar över samma UDP-anslutning (HTTP/3)
            EnableMultipleHttp2Connections = true,

            // Optimera för snabba "handskakningar"
            ConnectTimeout = TimeSpan.FromSeconds(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),

            // Keep-alive ser till att vi inte behöver förhandla TLS varje gång
            KeepAlivePingDelay = TimeSpan.FromSeconds(60),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(30)
        };

        return new HttpClient(handler)
        {
            // Vi ber specifikt om version 3.0 (QUIC/UDP)
            DefaultRequestVersion = HttpVersion.Version30,

            // Fallback till v2 eller v1.1 om routern/brandväggen i studion blockar UDP 443
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrLower
        };
    }
}