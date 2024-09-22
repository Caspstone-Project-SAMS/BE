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
    public event EventHandler<WebsocketEventArgs>? UpdateFingerprintEvent;
    public event EventHandler<WebsocketEventArgs>? CancelSessionEvent;
    public event EventHandler<WebsocketEventArgs>? PrepareAttendanceSession;
    public event EventHandler<WebsocketEventArgs>? StopAttendance;
    public event EventHandler<WebsocketEventArgs>? PrepareSchedules;
    public event EventHandler<WebsocketEventArgs>? CheckCurrentSession;
    public event EventHandler<WebsocketEventArgs>? StartAttendance;
    public event EventHandler<WebsocketEventArgs>? CheckUploadedScheduleEvent;
    public event EventHandler<WebsocketEventArgs>? SyncingAttendanceData;
    public event EventHandler<WebsocketEventArgs>? ApplyConfigurationsEvent;
    public event EventHandler<WebsocketEventArgs>? CheckInUseEvent;

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

    public void OnUpdateFingerprintEvent(string receivedEvent)
    {
        UpdateFingerprintEvent?.Invoke(this, new WebsocketEventArgs
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

    public void OnCheckCurrentSession(string receivedEvent)
    {
        CheckCurrentSession?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }

    public void OnStartAttendanceEvent(string receivedEvent)
    {
        StartAttendance?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }

    public void OnCheckUploadedScheduleEvent(string receivedEvent)
    {
        CheckUploadedScheduleEvent?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }

    public void OnSyncingAttendanceData(string receivedEvent)
    {
        SyncingAttendanceData?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }

    public void OnApplyConfigurationEvent(string receivedEvent)
    {
        ApplyConfigurationsEvent?.Invoke(this, new WebsocketEventArgs
        {
            Event = receivedEvent
        });
    }

    public void OnCheckInUseEvent(string receivedEvent)
    {
        CheckInUseEvent?.Invoke(this, new WebsocketEventArgs
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
