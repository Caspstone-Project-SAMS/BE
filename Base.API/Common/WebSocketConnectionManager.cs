using Duende.IdentityServer.Events;
using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Base.API.Common;

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
            var messageSend = new MessageSend
            {
                Event = "GetModuleID",
                Data = moduleId
            };
            var jsonPayload = JsonSerializer.Serialize(messageSend);
            var buffer = Encoding.UTF8.GetBytes(jsonPayload);
            await socket.SendAsync(
                new ArraySegment<byte>(buffer, 0, moduleId.Length),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
        );
        }
        _sockets.Add(new WebsocketClass
        {
            Socket = socket,
            IsRegisteredModule = isRegisterModule,
            ModuleId = moduleId
        });

    }

    public IList<WebSocket?> GetAllWebSockets()
    {
        return _sockets.Select(s => s.Socket).ToList();
    }

    public async void SendMessagesToAll(string? message)
    {
        if(message == null)
        {
            return;
        }
        var websockets = _sockets.Select(s => s.Socket);
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (WebSocket? ws in websockets)
        {
            if(ws != null)
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
        if(socket is null)
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



    private class WebsocketClass
    {
        public WebSocket? Socket { get; set; }
        public bool IsRegisteredModule { get; set; } = false;
        public string? ModuleId { get; set; }
    }

    private string generateModuleId()
    {
        string moduleId = "";
        Random r = new Random();
        for (int i = 0; i < 7; i++)
        {
            int n = r.Next(0, 25);
            char c = Convert.ToChar(n + 65);
            moduleId.Append(c);
        }
        return moduleId;
    }
}

public class MessageSend
{
    public string Event { get; set; } = string.Empty;
    public string Data { get; set; } = string.Empty;
}

public class DataSend
{
    public string studentID { get; set; } = string.Empty;
    public int status { get; set; }
}

