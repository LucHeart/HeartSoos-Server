using System.Net.WebSockets;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;

namespace LucHeart.HeartSoos;

public abstract class JsonWebsocketBaseController<T> : WebsocketBaseController<T>  where T : class
{
    private readonly IOptions<JsonOptions> _jsonOptions;

    public JsonWebsocketBaseController(ILogger<WebsocketBaseController<T>> logger, IHostApplicationLifetime lifetime, IOptions<JsonOptions> jsonOptions) : base(logger, lifetime)
    {
        _jsonOptions = jsonOptions;
    }
    
    
    protected override async Task Logic()
    {
        while (!Linked.IsCancellationRequested)
        {
            try
            {
                if (WebSocket!.State != WebSocketState.Open)
                {
                    Logger.LogDebug("Websocket state is not open, closing connection");
                    break;
                }
                var message =
                    await JsonWebSocketUtils.ReceiveFullMessageAsyncNonAlloc<T>(WebSocket,
                        _jsonOptions.Value.SerializerOptions, Linked.Token);

                if (message.IsT2)
                {
                    if (WebSocket.State != WebSocketState.Open)
                    {
                        Logger.LogWarning("Client sent closure, but connection state is not open");
                        break;
                    }

                    try
                    {
                        await WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Normal close",
                            Linked.Token);
                    }
                    catch (OperationCanceledException e)
                    {
                        Logger.LogError(e, "Error during close handshake");
                    }

                    Logger.LogInformation("Closing websocket connection");
                    break;
                }
                
                message.Switch(DataReceived,
                    failed => { Logger.LogWarning(failed.Exception, "Deserialization failed for websocket message"); },
                    _ => {  });
            }
            catch (OperationCanceledException)
            {
                Logger.LogInformation("WebSocket connection terminated due to close or shutdown");
                break;
            }
            catch (WebSocketException e)
            {
                if (e.WebSocketErrorCode != WebSocketError.ConnectionClosedPrematurely)
                    Logger.LogError(e, "Error in receive loop, websocket exception");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Exception while processing websocket request");
            }
        }

        await Close.CancelAsync();
    }
    
    protected abstract void DataReceived(T? wsRequest);
}