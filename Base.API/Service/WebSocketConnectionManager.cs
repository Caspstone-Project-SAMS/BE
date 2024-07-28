using Duende.IdentityServer.Events;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Base.API.Service;

public class WebSocketConnectionManager
{
    private IList<WebsocketClass> _sockets = new List<WebsocketClass>();

    public async Task AddSocket(WebSocket socket, bool isRegisterModule = false)
    {
        string? moduleId = null;
        if (isRegisterModule)
        {
            while (true)
            {
                moduleId = generateModuleId();
                if (!_sockets.Any(s => s.ModuleId == moduleId))
                {
                    break;
                }
            }
        }
        _sockets.Add(new WebsocketClass
        {
            Socket = socket,
            IsRegisteredModule = isRegisterModule,
            ModuleId = moduleId
        });

        if (isRegisterModule)
        {
            var messageSend = new WebsocketMessage
            {
                Event = "GetModuleID",
                Data = moduleId ?? ""
            };
            var jsonPayload = JsonSerializer.Serialize(messageSend);
            var buffer = Encoding.UTF8.GetBytes(jsonPayload);
            await socket.SendAsync(
                new ArraySegment<byte>(buffer, 0, moduleId?.Length ?? 0),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }
    }

    public IList<WebSocket?> GetAllWebSockets()
    {
        return _sockets.Select(s => s.Socket).ToList();
    }

    public IList<WebsocketClass> GetAllWebSocketsClass()
    {
        return _sockets.ToList();
    }

    public async void SendMessagesToAll(string? message)
    {
        if (message == null)
        {
            return;
        }
        var websockets = _sockets.Select(s => s.Socket);
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (WebSocket? ws in websockets)
        {
            if (ws != null)
            {
                await ws.SendAsync(
                    new ArraySegment<byte>(buffer, 0, message.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
            );
            }
        }
    }

    public async void SendMessageToModule(string? message, string? moduleId)
    {
        if (message is null || moduleId is null)
        {
            return;
        }
        var socket = _sockets.Where(s => s.ModuleId == moduleId).Select(s => s.Socket).FirstOrDefault();
        if (socket is null)
        {
            return;
        }

        var buffer = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(
           new ArraySegment<byte>(buffer, 0, message.Length),
           WebSocketMessageType.Text,
           true,
           CancellationToken.None
        );
    }

    public async Task CloseAllSocket()
    {
        var websocketsClass = _sockets;
        foreach (var ws in websocketsClass)
        {
            if (ws.Socket is not null)
            {
                await ws.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                _sockets.Remove(ws);
            }
        }

    }



    public class WebsocketClass
    {
        public WebSocket? Socket { get; set; }
        public bool IsRegisteredModule { get; set; } = false;
        public string? ModuleId { get; set; }
    }

    public class ModuleWebSocket
    {
        public WebSocket? Socket { get; set; }
        public string? ModuleID { get; set; }
    }

    public class ClientWebSocket
    {
        public WebSocket? Socket { get; set; }
        public Guid? UserID { get; set; }
    }

    private string generateModuleId()
    {
        string moduleId = "";
        Random r = new Random();
        for (int i = 0; i < 7; i++)
        {
            int n = r.Next(0, 26);
            char c = Convert.ToChar(n + 65);
            moduleId += c;
        }
        return moduleId;
    }
}

public class WebSocketConnectionManager1
{
    private IList<ModuleWebSocket> _moduleSockets = new List<ModuleWebSocket>();
    private IList<ClientWebSocket> _clientWebSocket = new List<ClientWebSocket>();

    public bool CheckModuleSocket(int moduleId)
    {
        var existedWebsocket = _moduleSockets.Where(s => s.ModuleID == moduleId).FirstOrDefault();
        if(existedWebsocket is null || existedWebsocket.Socket is null)
        {
            return false;
        }
        if(existedWebsocket.Socket.State == WebSocketState.Open)
        {
            return true;
        }
        return false;
    }

    public WebSocket? GetModuleSocket(int moduleId)
    {
        return _moduleSockets.FirstOrDefault(m => m.ModuleID == moduleId)?.Socket;
    }

    public WebSocket? GetClientSocket(Guid userId)
    {
        return _clientWebSocket.FirstOrDefault(c => c.UserID == userId)?.Socket;
    }

    public void AddModuleSocket(WebSocket socket, int moduleId)
    {
        var existedWebsocket = _moduleSockets.Where(s => s.ModuleID == moduleId).FirstOrDefault();
        if(existedWebsocket is null)
        {
            _moduleSockets.Add(new ModuleWebSocket
            {
                Socket = socket,
                ModuleID = moduleId
            });
        }
        else
        {
            existedWebsocket.Socket?.Dispose();
            existedWebsocket.Socket = socket;
        }
    }

    public void AddClientSocket(WebSocket socket, Guid userId)
    {
        var existedWebsocket = _clientWebSocket.FirstOrDefault(s => s.UserID == userId);
        if(existedWebsocket is null)
        {
            _clientWebSocket.Add(new ClientWebSocket
            {
                Socket = socket,
                UserID = userId
            });
        }
        else
        {
            existedWebsocket.Socket?.Dispose();
            existedWebsocket.Socket = socket;
        }
    }

    public async Task<bool> SendMesageToModule(string message, int moduleId)
    {
        var socket = _moduleSockets.Where(m => m.ModuleID == moduleId).FirstOrDefault()?.Socket;
        if (socket is null)
        {
            return false;
        }

        if (socket.State != WebSocketState.Open)
        {
            return false;
        }

        var buffer = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(
           new ArraySegment<byte>(buffer, 0, message.Length),
           WebSocketMessageType.Text,
           true,
           CancellationToken.None
        );

        return true;
    }

    public async Task<bool> SendMessageToClient(string message, Guid userId)
    {
        var socket = _clientWebSocket.Where(c => c.UserID == userId).FirstOrDefault()?.Socket;
        if (socket is null)
        {
            return false;
        }

        if (socket.State != WebSocketState.Open)
        {
            return false;
        }

        var buffer = Encoding.UTF8.GetBytes(message);
        await socket.SendAsync(
           new ArraySegment<byte>(buffer, 0, message.Length),
           WebSocketMessageType.Text,
           true,
           CancellationToken.None
        );

        return true;
    }

    public async void SendMessageToAllModule(string message)
    {
        var websockets = _moduleSockets.Select(s => s.Socket);
        if (websockets is null) return;
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (WebSocket? ws in websockets)
        {
            if (ws != null)
            {
                await ws.SendAsync(
                    new ArraySegment<byte>(buffer, 0, message.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
            );
            }
        }
    }

    public async void SendMessageToAllClient(string message)
    {
        var websockets = _clientWebSocket.Select(s => s.Socket);
        if (websockets is null) return;
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (WebSocket? ws in websockets)
        {
            if (ws != null)
            {
                await ws.SendAsync(
                    new ArraySegment<byte>(buffer, 0, message.Length),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None
            );
            }
        }
    }

    public async Task CloseModuleSocket(int moduleId, WebSocketCloseStatus? closeStatus, string? closeDescription, CancellationToken? cancellationToken)
    {
        var moduleSocket = _moduleSockets.Where(m => m.ModuleID == moduleId).FirstOrDefault();
        if (moduleSocket is null)
        {
            return;
        }
        var socket = moduleSocket.Socket;
        if (socket is not null && socket.State == WebSocketState.Open)
        {
            await socket.CloseAsync(closeStatus ?? WebSocketCloseStatus.NormalClosure, null, cancellationToken ?? CancellationToken.None);
            _moduleSockets.Remove(moduleSocket);
        }
        socket?.Dispose();
    }

    public async Task CloseClientSocket(Guid userId, WebSocketCloseStatus? closeStatus, string? closeDescription, CancellationToken? cancellationToken)
    {
        var clientSocket = _clientWebSocket.Where(c => c.UserID == userId).FirstOrDefault();
        if (clientSocket is null)
        {
            return;
        }
        var socket = clientSocket.Socket;
        if (socket is not null && socket.State == WebSocketState.Open)
        {
            await socket.CloseAsync(closeStatus ?? WebSocketCloseStatus.NormalClosure, null, cancellationToken ?? CancellationToken.None);
            _clientWebSocket.Remove(clientSocket);
        }
        socket?.Dispose();
    }

    public IEnumerable<ModuleWebSocket> GetAllModuleSocket()
    {
        return _moduleSockets;
    }

    public IEnumerable<ClientWebSocket> GetAllClientSocket()
    {
        return _clientWebSocket;
    }

    public class ModuleWebSocket
    {
        public WebSocket? Socket { get; set; }
        public int? ModuleID { get; set; }
    }

    public class ClientWebSocket
    {
        public WebSocket? Socket { get; set; }
        public Guid? UserID { get; set; }
    }
}


public class WebsocketMessage
{
    public string Event { get; set; } = string.Empty;
    public object? Data { get; set; }
}

public class DataSend
{
    public string studentID { get; set; } = string.Empty;
    public int status { get; set; }
}

