
/*
    THIS CODE IS NOT INTENDED TO BE USED PUBLIC, ONLY TEST THE IITenHandler interface
*/

using IntelligentAudio.API;
using Microsoft.Win32;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ChordFactory>();
builder.Services.AddSingleton<InferenceRegistry>();
builder.Services.AddHostedService<UdpOscReceiverService>();
builder.Services.AddSingleton(sp => new OscClient("127.0.0.1", 9005));

// Viktigt: Lyssna på 0.0.0.0 istället för bara localhost
builder.WebHost.ConfigureKestrel(options => { options.ListenAnyIP(5001);});
builder.Services.AddCors();

var app = builder.Build();

app.UseCors(policy => policy
    .WithOrigins("https://intelligentaudio.net", "https://api.intelligentaudio.net", "http://localhost:3000")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()); // Om du använder cookies/auth

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
app.MapGet("/v1/events/{sid:guid}", async (Guid sid, HttpContext context, InferenceRegistry registry, CancellationToken ct) =>
{
    // 1. Sätt rätt headers för SSE direkt
    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");

    // 2. Skapa en TaskCompletionSource om den inte redan finns (för säkerhets skull)
    var tcs = registry.PendingRequests.GetOrAdd(sid, _ => new TaskCompletionSource<int[]>());

    try
    {
        // 3. VÄNTA PÅ SVAR (Denna rad är magin!)
        // Tråden "sover" här tills UdpOscReceiverService anropar tcs.TrySetResult(notes)
        var notesTask = tcs.Task;
        var completedTask = await Task.WhenAny(notesTask, Task.Delay(10000, ct)); // 10s timeout

        if (completedTask == notesTask)
        {
            var notes = await notesTask;

            // 4. Skicka JSON till NextJS (måste sluta med \n\n för SSE)
            var json = JsonSerializer.Serialize(new { status = "success", notes });
            await context.Response.WriteAsync($"data: {json}\n\n", ct);
            await context.Response.Body.FlushAsync(ct);
        }
        else
        {
            await context.Response.WriteAsync("data: {\"status\": \"timeout\"}\n\n", ct);
        }
    }
    finally
    {
        // Städa upp efter oss
        registry.PendingRequests.TryRemove(sid, out _);
    }
});

app.MapPost("/ia/api/response", async (HttpContext context) =>
{
    //TODO create connection to MusicTheory.
    var mockNotes = new[] {
        new { note = "C4", velocity = 100 },
        new { note = "E4", velocity = 90 },
        new { note = "G4", velocity = 85 }
    };

    return Results.Ok(new { notes = mockNotes, chordName = "C Major" });

    // Här hamnar din logik för att hantera ljudet/datan
    // return Results.Ok(new { message = "Data mottagen!" });
});

app.Run();
