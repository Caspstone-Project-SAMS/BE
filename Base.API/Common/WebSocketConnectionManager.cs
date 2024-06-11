using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;

namespace Base.API.Common;

public class WebSocketConnectionManager
{
    private IList<WebSocket> _sockets = new List<WebSocket>();

    public void AddSocket(WebSocket socket)
    {
        _sockets.Add(socket);
    }

    public IList<WebSocket> GetAllWebSockets()
    {
        return _sockets;
    }

    public async void SendMessagesToAll(string? message)
    {
        if(message == null)
        {
            return;
        }
        var buffer = Encoding.UTF8.GetBytes(message);
        foreach (WebSocket ws in _sockets)
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
