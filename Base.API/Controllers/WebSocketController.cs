using Base.API.Service;
using Base.Service.Common;
using Base.Service.IService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using static Org.BouncyCastle.Math.EC.ECCurve;
using Base.API.Common;

namespace Base.API.Controllers;

public class WebSocketController : ControllerBase
{
    private readonly WebSocketConnectionManager _websocketConnectionManager;
    private readonly WebSocketConnectionManager1 _websocketConnectionManager1;
    private readonly IModuleService _moduleService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly SessionManager _sessionManager;
    private readonly WebsocketEventManager _websocketEventManager;

    private event EventHandler<WebsocketEventArgs>? PingPongEvent;
    private bool _pingPongStatus = false;

    public WebSocketController(WebSocketConnectionManager webSocketConnectionManager, 
        WebSocketConnectionManager1 websocketConnectionManager1,
        IModuleService moduleService,
        ICurrentUserService currentUserService,
        IUserService userService,
        SessionManager sessionManager,
        WebsocketEventManager websocketEventManager)
    {
        _websocketConnectionManager = webSocketConnectionManager;
        _websocketConnectionManager1 = websocketConnectionManager1;
        _moduleService = moduleService;
        _currentUserService = currentUserService;
        _userService = userService;
        _sessionManager = sessionManager;
        _websocketEventManager = websocketEventManager;
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

                // Create a manager for the event handlers of that module's websocket based on moduleId
                _websocketEventManager.AddHandler(existedModule.ModuleID);
                var websocketEventHandler = _websocketEventManager.GetHandlerByModuleID(existedModule.ModuleID);

                var buffer = new byte[1024 * 4];
                var receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);

                while (!receiveResult.CloseStatus.HasValue)
                {
                    receiveResult = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer), CancellationToken.None);
                    string receiveData = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);
                    if (receiveResult.MessageType == WebSocketMessageType.Binary)
                    {
                        if(receiveData == "pong")
                        {
                            PingPongEvent?.Invoke(this, new WebsocketEventArgs
                            {
                                Event = "PingPong"
                            });
                        }
                    }
                    else if(receiveResult.MessageType == WebSocketMessageType.Text)
                    {
                        if (receiveData.Contains("Connected"))
                        {
                            if(websocketEventHandler is not null)
                                websocketEventHandler.OnConnectModuleEvent(receiveData);
                        }
                        else if(receiveData.Contains("Fingerprint registration"))
                        {
                            if (websocketEventHandler is not null)
                                websocketEventHandler.OnRegisterFingerprintEvent(receiveData);
                        }
                    }
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
        var a = new WebsocketEventArgs();
        PingPongEvent += OnPingPongEventHandler;
        while (webSocket.State == WebSocketState.Open)
        {
            try
            {
                // Send a ping frame with unique payload
                var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes("ping"));
                await webSocket.SendAsync(buffer, WebSocketMessageType.Binary, true, CancellationToken.None);

                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(10));

                var pongReceived = WaitForPong(cts.Token);

                if (!pongReceived)
                {
                    webSocket.Abort();
                    //await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Pong not received", CancellationToken.None);
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
        PingPongEvent -= OnPingPongEventHandler;
    }

    private bool WaitForPong(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_pingPongStatus)
                {
                    _pingPongStatus = false;
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }

    private void OnPingPongEventHandler(object? sender, WebsocketEventArgs e)
    {
        if(e.Event == "PingPong")
        {
            _pingPongStatus = true;
        }
    }
}
