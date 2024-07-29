namespace Base.API.Common;

public class WebsocketEventManager
{
    private IList<WebsocketEventHandler> _websocketEventHandlers = new List<WebsocketEventHandler>();

    public void AddHandler(int moduleId)
    {
        var existedWebsocketEventHandler = _websocketEventHandlers.FirstOrDefault(h => h.ModuleId == moduleId);
        var newWebsocketEventHandler = new WebsocketEventHandler
        {
            ModuleId = moduleId,
        };
        if (existedWebsocketEventHandler != null)
        {
            _websocketEventHandlers.Remove(existedWebsocketEventHandler);
        }
        _websocketEventHandlers.Add(newWebsocketEventHandler);
    }

    public WebsocketEventHandler? GetHandlerByModuleID(int moduleId)
    {
        return _websocketEventHandlers.FirstOrDefault(h => h.ModuleId == moduleId);
    }
}

public class WebsocketEventHandler
{
    public int ModuleId { get; set; }

    public event EventHandler<WebsocketEventArgs>? ConnectModuleEvent;
    public event EventHandler<WebsocketEventArgs>? RegisterFingerprintEvent;
    public event EventHandler<WebsocketEventArgs>? CancelSessionEvent;
    public event EventHandler<WebsocketEventArgs>? PrepareAttendanceSession;
    public event EventHandler<WebsocketEventArgs>? StopAttendance;
    public event EventHandler<WebsocketEventArgs>? PrepareSchedules;

    public void OnConnectModuleEvent(string receivedEvent)
    {
        ConnectModuleEvent?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }

    public void OnRegisterFingerprintEvent(string receivedEvent)
    {
        RegisterFingerprintEvent?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }

    public void OnCancelSessionEvent(string receivedEvent)
    {
        CancelSessionEvent?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }

    public void OnPrepareAttendanceSession(string receivedEvent)
    {
        PrepareAttendanceSession?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }

    public void OnStopAttendanceEvent(string receivedEvent)
    {
        StopAttendance?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }

    public void OnPrepareSchedules(string receivedEvent)
    {
        PrepareSchedules?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }
}

public class WebsocketEventArgs
{
    public string Event { get; set; } = string.Empty;
    public object? Data { get; set; }
}
