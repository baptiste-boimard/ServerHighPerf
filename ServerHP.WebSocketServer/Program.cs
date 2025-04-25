using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ServerHP.WebSocketServer;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.websocket.json", optional: false);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5287); 
});

var app = builder.Build();

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[1024 * 64];
        var ct = context.RequestAborted;

        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), ct);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("[Serveur] Demande de fermeture reçue.");
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", ct);
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var json = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"[Serveur] Message reçu : {json}");

                    var msg = JsonSerializer.Deserialize<MessageInfoWebSocket>(json);

                    if (msg != null)
                    {
                        msg.ReceivedAt = DateTime.UtcNow;

                        var responseJson = JsonSerializer.Serialize(msg);
                        var responseBytes = Encoding.UTF8.GetBytes(responseJson);

                        await webSocket.SendAsync(
                            new ArraySegment<byte>(responseBytes),
                            WebSocketMessageType.Text,
                            true,
                            ct);
                    }
                }
            }
        }
        catch (WebSocketException ex)
        {
            Console.WriteLine($"[WS Error] Fermeture inattendue : {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Erreur] {ex.Message}");
        }
        finally
        {
            if (webSocket.State != WebSocketState.Closed && webSocket.State != WebSocketState.Aborted)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Fermeture serveur", ct);
            }

            webSocket.Dispose();
        }
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.Run();
