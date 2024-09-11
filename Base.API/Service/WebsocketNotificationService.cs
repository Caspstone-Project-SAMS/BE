using Base.Service.Common;

namespace Base.API.Service;

public class WebsocketNotificationService : IWebsocketNotificationService
{
    private readonly WebSocketConnectionManager1 _webSocketConnectionManager;
    public WebsocketNotificationService(WebSocketConnectionManager1 webSocketConnectionManager)
    {
        _webSocketConnectionManager = webSocketConnectionManager;
    }
    public async Task<bool> SendMessageToClient(string message, Guid userId)
    {
        return await _webSocketConnectionManager.SendMessageToRootClient(message, userId);
    }
}
