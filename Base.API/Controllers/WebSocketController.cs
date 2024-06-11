using Base.API.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Base.API.Controllers;

public class WebSocketController : ControllerBase
{
    private readonly WebSocketConnectionManager _websocketConnectionManager;

    public WebSocketController(WebSocketConnectionManager webSocketConnectionManager)
    {
        _websocketConnectionManager = webSocketConnectionManager;
    }

    [HttpGet("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            //var connId = HttpContext.Connection.Id;
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            _websocketConnectionManager.AddSocket(webSocket);

            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }


}
