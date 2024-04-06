using System.Threading.Channels;
using LucHeart.HeartSoos.Models;
using LucHeart.HeartSoos.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace LucHeart.HeartSoos.Controller;

[ApiController]
[Route("/ws")]
public sealed class WsHeartSoosController : JsonWebsocketBaseController<HeartSoosWsData>
{
    private static readonly List<WsHeartSoosController> ReceiverWebsockets = new();

    private readonly Channel<HeartSoosWsData> _channel =
        Channel.CreateUnbounded<HeartSoosWsData>();

    public WsHeartSoosController(ILogger<WebsocketBaseController<HeartSoosWsData>> logger, IHostApplicationLifetime lifetime, IOptions<JsonOptions> jsonOptions) : base(logger, lifetime, jsonOptions)
    {
    }

    public override string Id => _id;

    private string _id;

    #region Input

    [HttpGet("{id}")]
    public async Task Get(string id)
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        _id = id;
        WebSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        await Logic();
    }
    
    protected override void DataReceived(HeartSoosWsData? wsRequest)
    {
        LucTask.Run(DataReceivedTask(wsRequest));
    }

    private async Task DataReceivedTask(HeartSoosWsData? wsRequest)
    {
        if (wsRequest == null) return;
        HeartRateManager.SetHeartRate(Id, wsRequest.HeartRate);
        Logger.LogInformation("HeartRate: {Id} - {HeartRate}", Id, wsRequest.HeartRate);

        foreach (var ws in ReceiverWebsockets.Where(x => x.Id == Id)) await ws.QueueMessage(wsRequest);
    }

    #endregion

}