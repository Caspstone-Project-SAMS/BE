using Base.API.Common;
using Base.Service.Common;
using Base.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Base.API.Controllers;

public class WebSocketController : ControllerBase
{
    private readonly WebSocketConnectionManager _websocketConnectionManager;
    private readonly WebSocketConnectionManager1 _websocketConnectionManager1;
    private readonly IModuleService _moduleService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public WebSocketController(WebSocketConnectionManager webSocketConnectionManager, 
        WebSocketConnectionManager1 websocketConnectionManager1,
        IModuleService moduleService,
        ICurrentUserService currentUserService,
        IUserService userService)
    {
        _websocketConnectionManager = webSocketConnectionManager;
        _websocketConnectionManager1 = websocketConnectionManager1;
        _moduleService = moduleService;
        _currentUserService = currentUserService;
        _userService = userService;
    }

    [HttpGet("/ws")]
    public async Task Get([FromQuery] bool isRegisterModule = false)
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            //var connId = HttpContext.Connection.Id;
            using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
            {
                await _websocketConnectionManager.AddSocket(webSocket, isRegisterModule);

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
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }


    [HttpGet("/ws/module")]
    public async Task GetModule([FromQuery] string key)
    {
        var getModulesResult = await _moduleService.Get(1, 1, 1, null, null, key, null);
        var existedModule = getModulesResult.Result?.FirstOrDefault();
        if (HttpContext.WebSockets.IsWebSocketRequest && existedModule is not null && existedModule.ModuleID > 0)
        {
            using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
            {
                _websocketConnectionManager1.AddModuleSocket(webSocket, existedModule.ModuleID);

                var buffer = new byte[1024 * 4];
                var receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!receiveResult.CloseStatus.HasValue)
                {
                    receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await _websocketConnectionManager1.CloseModuleSocket(existedModule.ModuleID,
                    receiveResult.CloseStatus.Value,
                    receiveResult.CloseStatusDescription,
                    CancellationToken.None);
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }


    [HttpGet("/ws/client")]
    public async Task GetClient()
    {
        var userId = _currentUserService.UserId;
        var currentUser = await _userService.GetUserById(new Guid(userId));
        if (HttpContext.WebSockets.IsWebSocketRequest && currentUser is not null)
        {
            using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
            {
                _websocketConnectionManager1.AddClientSocket(webSocket, currentUser.Id);

                var buffer = new byte[1024 * 4];
                var receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!receiveResult.CloseStatus.HasValue)
                {
                    receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                await _websocketConnectionManager1.CloseClientSocket(currentUser.Id,
                    receiveResult.CloseStatus.Value,
                    receiveResult.CloseStatusDescription,
                    CancellationToken.None);
            }
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
