using Base.API.Service;
using Base.Service.Common;
using Base.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace Base.API.Controllers;

public class WebSocketController : ControllerBase
{
    private readonly WebSocketConnectionManager _websocketConnectionManager;
    private readonly WebSocketConnectionManager1 _websocketConnectionManager1;
    private readonly IModuleService _moduleService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly SessionManager _sessionManager;

    public WebSocketController(WebSocketConnectionManager webSocketConnectionManager, 
        WebSocketConnectionManager1 websocketConnectionManager1,
        IModuleService moduleService,
        ICurrentUserService currentUserService,
        IUserService userService,
        SessionManager sessionManager)
    {
        _websocketConnectionManager = webSocketConnectionManager;
        _websocketConnectionManager1 = websocketConnectionManager1;
        _moduleService = moduleService;
        _currentUserService = currentUserService;
        _userService = userService;
        _sessionManager = sessionManager;
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
        if(key is null || key == "" || key == string.Empty)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
        var getModulesResult = await _moduleService.Get(1, 1, 1, null, null, key, null);
        var existedModule = getModulesResult.Result?.FirstOrDefault();
        if (HttpContext.WebSockets.IsWebSocketRequest && existedModule is not null && existedModule.ModuleID > 0)
        {
            using (var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync())
            {
                // Call the keep-alive function in a separate task
                _ = KeepAlive(webSocket);

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
        if(userId == "Undefined")
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }
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

                    if(receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        string? jsonString = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                        WebsocketMessage? receivedMessage = JsonSerializer.Deserialize<WebsocketMessage>(jsonString);
                        if(receivedMessage is not null)
                        {
                            var sendMessage = new WebsocketMessage();

                            switch (receivedMessage.Event)
                            {
                                case "CheckModule":
                                    var checkModuleResult = CheckModule(receivedMessage);
                                    if(checkModuleResult is not null)
                                    {
                                        await _websocketConnectionManager1.SendMessageToClient(checkModuleResult, currentUser.Id);
                                    }
                                    break;
                                case "CheckModules":
                                    var checkModulesResult = CheckModulesResult(receivedMessage);
                                    if(checkModulesResult is not null)
                                    {
                                        await _websocketConnectionManager1.SendMessageToClient(checkModulesResult, currentUser.Id);
                                    }
                                    break;
                            }
                        }
                    }
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


    private string? CheckModule(WebsocketMessage receivedMessage)
    {
        var moduleId = (int?)receivedMessage.Data;
        if (moduleId is null) return null;
        receivedMessage.Event = "CheckModuleResult";
        receivedMessage.Data = new
        {
            ModuleId = moduleId,
            Result = _websocketConnectionManager1.CheckModuleSocket((int)moduleId)
        };
        return JsonSerializer.Serialize(receivedMessage);
    }

    private string? CheckModulesResult(WebsocketMessage receivedMessage)
    {
        var moduleIds = (List<int>?)receivedMessage.Data;
        if (moduleIds is null || moduleIds.Count <= 0) return null;
        var checkModulesResult = new List<object>();
        foreach (var moduleId in moduleIds)
        {
            var moduleResult = new
            {
                ModuleId = moduleId,
                Result = _websocketConnectionManager1.CheckModuleSocket((int)moduleId)
            };
            checkModulesResult.Add(moduleResult);
        }
        receivedMessage.Event = "CheckModulesResult";
        receivedMessage.Data = checkModulesResult;
        return JsonSerializer.Serialize(receivedMessage);
    }

    private async Task KeepAlive(WebSocket webSocket)
    {
        await Task.Delay(TimeSpan.FromSeconds(30));
        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(10));

                // Send a ping frame with unique payload
                var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes("ping"));
                await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);

                var pongReceived = await WaitForPongAsync(webSocket, cts.Token);

                if (!pongReceived)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Pong not received", CancellationToken.None);
                    webSocket.Dispose();
                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(10)); // Ping interval
            }
            catch (Exception ex)
            {
                Console.WriteLine($"KeepAlive exception: {ex}");
                break;
            }
        }
    }

    private async Task<bool> WaitForPongAsync(WebSocket webSocket, CancellationToken cancellationToken)
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                if(result.MessageType == WebSocketMessageType.Binary)
                {
                    string receiveData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine(receiveData);
                    _sessionManager.AddString(receiveData);
                    if (receiveData == "pong")
                    {
                        return true;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }
}
