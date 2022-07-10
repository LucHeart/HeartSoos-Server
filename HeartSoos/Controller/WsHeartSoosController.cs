using System.Net.WebSockets;
using System.Text;
using LucHeart.HeartSoos.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace LucHeart.HeartSoos.Controller;

[ApiController]
[Route("/ws")]
public class WsHeartSoosController : ControllerBase
{

    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _lifetime;
    
    public WsHeartSoosController(ILogger<WsHeartSoosController> logger, IHostApplicationLifetime lifetime)
    {
        _logger = logger;
        _lifetime = lifetime;
    }

    [HttpGet("{id}")]
    public async Task Get(string id)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await Logic(websocket, id);
    }
    
    private async Task Logic(WebSocket webSocket, string id)
    {
        WebSocketReceiveResult result;
        do
        {
            var message = await ReceiveFullMessage(webSocket, _lifetime.ApplicationStopped);
            result = message.Item1;
            
            var dec = Encoding.UTF8.GetString(message.Item2.ToArray());
            try
            {
                var json = JsonConvert.DeserializeObject<HeartSoosWsData>(dec);
                if(json == null) continue;
                HeartRateManager.SetHeartRate(id, json.HeartRate);
            }
            catch (JsonException e)
            {
                _logger.LogError(e, "Error deserializing json, {Json}", dec);
            }
        } while (!result.CloseStatus.HasValue);
        
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        _logger.LogInformation("WebSocket connection closed");
    }
    
    private static async Task<(WebSocketReceiveResult, IEnumerable<byte>)> ReceiveFullMessage(
        WebSocket socket, CancellationToken cancelToken)
    {
        WebSocketReceiveResult response;
        var message = new List<byte>();

        var buffer = new byte[4096];
        do
        {
            response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), cancelToken);
            message.AddRange(new ArraySegment<byte>(buffer, 0, response.Count));
        } while (!response.EndOfMessage);

        return (response, message);
    }

}