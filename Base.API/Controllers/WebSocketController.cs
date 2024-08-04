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
using System.Linq;

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
    private readonly IServiceScopeFactory _serviceScopeFactory;

    private event EventHandler<WebsocketEventArgs>? PingPongEvent;
    private bool _pingPongStatus = false;
    private int currentSessionID = 0;

    public WebSocketController(WebSocketConnectionManager webSocketConnectionManager, 
        WebSocketConnectionManager1 websocketConnectionManager1,
        IModuleService moduleService,
        ICurrentUserService currentUserService,
        IUserService userService,
        SessionManager sessionManager,
        WebsocketEventManager websocketEventManager,
        IServiceScopeFactory serviceScopeFactory)
    {
        _websocketConnectionManager = webSocketConnectionManager;
        _websocketConnectionManager1 = websocketConnectionManager1;
        _moduleService = moduleService;
        _currentUserService = currentUserService;
        _userService = userService;
        _sessionManager = sessionManager;
        _websocketEventManager = websocketEventManager;
        _serviceScopeFactory = serviceScopeFactory;
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
                // Notify module is connected
                _ = NotifyModuleConnected(existedModule.ModuleID);

                // Call the keep-alive function in a separate task
                _ = KeepAlive(webSocket, existedModule.ModuleID);

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
                        if (receiveData.Contains("Connected") || receiveData == "Connected by other")
                        {
                            if(websocketEventHandler is not null)
                                websocketEventHandler.OnConnectModuleEvent(receiveData);
                        }
                        else if (receiveData.Contains("Register fingerprint"))
                        {
                            if (websocketEventHandler is not null)
                                websocketEventHandler.OnRegisterFingerprintEvent(receiveData);
                        }
                        else if (receiveData.Contains("Update fingerprint"))
                        {
                            if (websocketEventHandler is not null)
                                websocketEventHandler.OnUpdateFingerprintEvent(receiveData);
                        }
                        else if (receiveData.Contains("Prepare attendance"))
                        {
                            if(websocketEventHandler is not null)
                            {
                                websocketEventHandler.OnPrepareAttendanceSession(receiveData);
                            }
                        }
                        else if (receiveData.Contains("Prepare schedules"))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.OnPrepareSchedules(receiveData);
                            }
                        }
                        else if (receiveData.Contains("Cancel session"))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.OnCancelSessionEvent(receiveData);
                            }
                        }
                        else if (receiveData.Contains("Stop attendance"))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.OnStopAttendanceEvent(receiveData);
                            }
                        }
                        else if (receiveData.Contains("Start attendance"))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.OnStartAttendanceEvent(receiveData);
                            }
                        }
                        else if (receiveData.Contains("Check current session"))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.OnCheckCurrentSession(receiveData);
                            }
                        }
                        else if (receiveData.Contains("Check uploaded schedule"))
                        {
                            if (websocketEventHandler is not null)
                            {
                                websocketEventHandler.OnCheckUploadedScheduleEvent(receiveData);
                            }
                        }
                        else if (receiveData.Contains("Check current session"))
                        {
                            currentSessionID = int.Parse(receiveData.Split(" ").LastOrDefault() ?? "0");
                        }
                        else if (receiveData.Contains("Session cancelled"))
                        {
                            // Module cancel session itself, because it waited for action too long
                            int sessionId = int.Parse(receiveData.Split(" ").LastOrDefault() ?? "0");
                            if (sessionId > 0)
                            {
                                var session = _sessionManager.GetSessionById(sessionId);
                                if (session is not null)
                                {
                                    _sessionManager.CancelSession(sessionId, session.UserID);
                                    var messageSend = new WebsocketMessage
                                    {
                                        Event = "CancelSession",
                                        Data = new
                                        {
                                            SessionID = sessionId,
                                            ModuleID = session
                                        }
                                    };
                                    var jsonPayload = JsonSerializer.Serialize(messageSend);
                                    await _websocketConnectionManager1.SendMessageToClient(jsonPayload, session.UserID);
                                }
                            }
                        }
                        else if (receiveData.Contains("Schedule preparation completed"))
                        {
                            var splitStrings = receiveData.Split(";;;");
                            var sessionId = int.Parse(splitStrings[1] ?? "0");
                            var uploadedFingers = int.Parse(splitStrings[2] ?? "0");
                            if (receiveData.Contains("successfully"))
                            {
                                _ = _sessionManager.CompleteSession(sessionId, true, uploadedFingers);
                            }
                            else if (receiveData.Contains("failed"))
                            {
                                _ = _sessionManager.CompleteSession(sessionId, false);
                            }
                        }
                        else if (receiveData.Contains("Fingerprint registration completed"))
                        {
                            var sessionId = int.Parse(receiveData.Split(" ").LastOrDefault() ?? "0");
                            // finish session to ready to submit
                            _sessionManager.FinishSession(sessionId);
                        }
                        else if (receiveData.Contains("Fingerprint update completed"))
                        {
                            var sessionId = int.Parse(receiveData.Split(" ").LastOrDefault() ?? "0");
                            // finish session to ready to submit
                            _sessionManager.FinishSession(sessionId);
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

    private async Task KeepAlive(WebSocket webSocket, int moduleId)
    {
        await Task.Delay(TimeSpan.FromSeconds(10));
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
                    webSocket.Dispose();

                    await Task.Delay(TimeSpan.FromSeconds(1));

                    // Notify to user that module is lost connected
                    _ = NotifyModuleLostConnected(moduleId);

                    // If the connection is lost, lets end/complete all ongoing session after 1 min
                    // For fingerprint registration, lets just end it
                    // For preparation, complete it
                    _ = HandleSessionAfterConnectionLost(moduleId);

                    break;
                }

                await Task.Delay(TimeSpan.FromSeconds(7)); // Ping interval
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

    private async Task NotifyModuleLostConnected(int moduleId)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var moduleService = serviceScope.ServiceProvider.GetRequiredService<IModuleService>();

        var module = await moduleService.GetById(moduleId);
        if (module is null || module.Employee?.User is null) return;
        var clientSocket = _websocketConnectionManager1.GetClientSocket(module.Employee.User.Id);
        if (clientSocket is null) return;
        var messageSend = new WebsocketMessage
        {
            Event = "ModuleLostConnected",
            Data = new
            {
                ModuleId = moduleId
            }
        };
        var jsonPayload = JsonSerializer.Serialize(messageSend);
        _ = _websocketConnectionManager1.SendMessageToClient(jsonPayload, module.Employee.User.Id);
    }

    private async Task NotifyModuleConnected(int moduleId)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var moduleService = serviceScope.ServiceProvider.GetRequiredService<IModuleService>();

        var module = await moduleService.GetById(moduleId);
        if (module is null || module.Employee?.User is null) return;
        var clientSocket = _websocketConnectionManager1.GetClientSocket(module.Employee.User.Id);
        if (clientSocket is null) return;
        var messageSend = new WebsocketMessage
        {
            Event = "ModuleConnected",
            Data = new
            {
                ModuleId = moduleId
            }
        };
        var jsonPayload = JsonSerializer.Serialize(messageSend);

        await Task.Delay(TimeSpan.FromSeconds(10));
        _ = _websocketConnectionManager1.SendMessageToClient(jsonPayload, module.Employee.User.Id);
    }

    private async Task HandleSessionAfterConnectionLost(int moduleId)
    {
        // Chưa lấy state = 0, thực hiện sau 
        var session = _sessionManager.GetSessions(null, 1, null, moduleId, null).FirstOrDefault();
        if (session is not null)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMinutes(1));
            await HandleSessionCancelled(session, cts.Token);
        }
    }

    private async Task HandleSessionCancelled(Session session, CancellationToken cancellationToken)
    {
        try
        {
            var cts = new CancellationTokenSource();
            while (!cancellationToken.IsCancellationRequested)
            {
                var moduleSocket = _websocketConnectionManager1.GetModuleSocket(session.ModuleId);
                if (moduleSocket is not null)
                {
                    // Check whether if the module still holding the session?
                    var messageSend = new WebsocketMessage
                    {
                        Event = "CheckCurrentSession",
                        Data = null
                    };
                    var jsonPayload = JsonSerializer.Serialize(messageSend);
                    var sendResult = await _websocketConnectionManager1.SendMesageToModule(jsonPayload, session.ModuleId);
                    if (sendResult)
                    {
                        cts.CancelAfter(TimeSpan.FromSeconds(6));
                        if (WaitForCheckCurrentSession(cts.Token, session.SessionId))
                        {
                            currentSessionID = 0;
                            return;
                        }
                    }
                }
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
        catch (OperationCanceledException)
        {
        }

        // Module do not reconnect, or module lost the session track => lets cancel the session
        if (session.Category == 1)
        {
            _sessionManager.FinishSession(session.SessionId);
        }
        else if (session.Category == 2 || session.Category == 3 || session.Category == 4)
        {
            await _sessionManager.CompleteSession(session.SessionId, false);
        }
    }

    private bool WaitForCheckCurrentSession(CancellationToken cancellationToken, int sessionId)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if(currentSessionID == sessionId)
                {
                    return true;
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        return false;
    }
}
