
/*
    THIS CODE IS NOT INTENDED TO BE USED PUBLIC, ONLY TEST THE IITenHandler interface
*/

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ChordFactory>();
builder.Services.AddSingleton(new OscClient("127.0.0.1", 9005));
// Viktigt: Lyssna på 0.0.0.0 istället för bara localhost
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5001);});

var app = builder.Build();
app.UseRouting();

//Not in production PLEASE.
app.MapGet("/", () => Results.Text("api.intelligentaudio.net is LIVE", "text/plain"));


// Simple test 
app.MapGet("/v1/chord", (
    [FromQuery] string q,
    [FromQuery] Guid sid,
    [FromServices] ChordFactory factory,
    [FromServices] OscClient osc) =>
{
    int[] notes = factory.Parse(q);

    if (notes != null && notes.Length > 0)
    {
        // 1. Hämta writern från din OscClient
        var writer = osc.Writer;
        writer.Reset();

        // 2. Skriv adressen
        writer.Write("/ia/api/request");

        // 3. Skriv tag-strängen (s för string, i för int)
        // OBS: I OSC börjar tag-strängen alltid med ett kommatecken
        writer.Write(",siiii");

        // 4. Skriv själva datan i exakt den ordning taggarna anger
        writer.Write(sid.ToString()); // s

        for (int i = 0; i < 4; i++)
        {
            int val = i < notes.Length ? notes[i] : 0;
            writer.Write(val); // i, i, i, i
        }

        // 5. Skicka det färdiga paketet direkt via socketen
        // Detta är det snabbaste sättet i .NET 10
        osc.Socket.Send(writer.Buffer, writer.Length, System.Net.Sockets.SocketFlags.None);


        return Results.Json(new { status = "ok", message = "intent_dispatched" }, statusCode: 202);


       // return Results.Accepted();
    }

    return Results.BadRequest();
});

// SSE-endpointen för response
app.MapGet("/v1/events/{sid:guid}", async (Guid sid, HttpContext context, CancellationToken ct) =>
{
   
});

app.Run();
