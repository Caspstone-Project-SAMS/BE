using Base.API.Common;
using Base.API.Controllers;
using Base.IService.IService;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Hangfire;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Base.API.Service;

public class HangfireService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly WebSocketConnectionManager1 _websocketConnectionManager;
    private readonly SessionManager _sessionManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly WebsocketEventManager _websocketEventManager;
    private readonly WebsocketEventState websocketEventState = new WebsocketEventState();
    private readonly IScheduleService _scheduleService;
    private readonly IStudentService _studentService;
    private readonly IModuleService _moduleService;

    public HangfireService(IBackgroundJobClient backgroundJobClient,
                           IRecurringJobManager recurringJobManager,
                           WebSocketConnectionManager1 websocketConnectionManager,
                           SessionManager sessionManager,
                           ICurrentUserService currentUserService,
                           WebsocketEventManager websocketEventManager,
                           IScheduleService scheduleService,
                           IStudentService studentService,
                           IModuleService moduleService)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _websocketConnectionManager = websocketConnectionManager;
        _sessionManager = sessionManager;
        _currentUserService = currentUserService;
        _websocketEventManager = websocketEventManager;
        _scheduleService = scheduleService;
        _studentService = studentService;
        _moduleService = moduleService;
    }

    public void ConfigureRecurringJobsAsync(string jobName, TimeOnly? prepareTime, int moduleId)
    {
        DateTime vnDateTime = ServerDateTime.GetVnDateTime();
        DateOnly date = DateOnly.FromDateTime(vnDateTime);
        if (prepareTime.HasValue && prepareTime.Value >= new TimeOnly(19, 0) && prepareTime.Value < new TimeOnly(0, 0))
        {
            date = DateOnly.FromDateTime(vnDateTime.AddDays(1));
        }
        else
        {
            date = DateOnly.FromDateTime(vnDateTime);
        }


        var cronExpression = ConvertToCronExpression(prepareTime);

        _recurringJobManager.AddOrUpdate(
            jobName,
            () => SetupPreparationForModule(date, moduleId),
            cronExpression,
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"),
            }
        );

    }

    public void RemoveRecurringJobsAsync(string jobId)
    {
        _recurringJobManager.RemoveIfExists(jobId);
    }

    public async Task<string> SetupPreparationForModule(DateOnly date, int moduleId)
    {
        var sessionId = await ConnectModule(moduleId);
        if(sessionId is null)
        {
            return "Module not connected";
        }

        var result = await StartPrepareSchedules(sessionId ?? 0, date);

        if (result)
        {
            return $"Prepare schedules for module unsuccessfully";
        }
        return $"Prepare schedules for module unsuccessfully";


        /*var messageSendMode = new WebsocketMessage
        {
            Event = "PrepareSchedules",
            Data = new
            {
                PrepareDate = date?.ToString("yyyy-MM-dd")
            },
        };
        var jsonPayloadMode = JsonSerializer.Serialize(messageSendMode);
        var resultMode = await _websocketConnectionManager.SendMesageToModule(jsonPayloadMode,moduleId);

        try
        {
            if (resultMode)
            {
                return $"Prepare schedules for module unsuccessfully";
            }
            return $"Prepare schedules for module unsuccessfully, data {jsonPayloadMode}";
        }
        catch (Exception ex)
        {
            return $"Error sending data to module: {ex.Message}";
        }*/
    }

    private static string ConvertToCronExpression(TimeOnly? prepareTime)
    {
        if (prepareTime.HasValue)
        {
            TimeOnly time = prepareTime.Value;
            return Cron.Daily(time.Hour, time.Minute);
        }
        else
        {
            return Cron.Daily();
        }
    }





    public async Task<int?> ConnectModule(int moduleId)
    {
        var websocketEventHandler = _websocketEventManager.GetHandlerByModuleID(moduleId);

        var module = await _moduleService.GetById(moduleId);
        var userId = module?.Employee?.User?.Id ?? Guid.Empty;

        var sessionId = _sessionManager.CreateSession(moduleId, userId, 1);
        if (websocketEventHandler is not null)
        {
            websocketEventHandler.ConnectModuleEvent += OnModuleConnectingEventHandler;
        }
        websocketEventState.SessionId = sessionId;

        var messageSend = new WebsocketMessage
        {
            Event = "ConnectModule",
            Data = new
            {
                SessionID = sessionId,
                User = _currentUserService.UserName,
                DurationInMin = 1
            }
        };
        var jsonPayload = JsonSerializer.Serialize(messageSend);
        var result = await _websocketConnectionManager.SendMesageToModule(jsonPayload, moduleId);
        if (result)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            if (WaitForModuleConnecting(cts.Token))
            {
                if (websocketEventHandler is not null)
                {
                    websocketEventHandler.ConnectModuleEvent -= OnModuleConnectingEventHandler;
                }

                return sessionId;
            }
        }
        _sessionManager.DeleteSession(sessionId);

        if (websocketEventHandler is not null)
        {
            websocketEventHandler.ConnectModuleEvent -= OnModuleConnectingEventHandler;
        }
        return null;
    }

    public async Task<bool> StartPrepareSchedules(int sessionId, DateOnly preparedDate)
    {
        #region test
        /*var getSchedulesResult = await _scheduleService.GetAllSchedules(1, 100, 100, null, null, preparedDate, preparedDate);
        if (!getSchedulesResult.IsSuccess) return false;

        var schedules = getSchedulesResult?.Result?.ToHashSet();
        if (schedules is null) return false;

        var classeIds = schedules.Where(s => s.Class != null).Select(s => s.ClassID).ToHashSet();

        if (classeIds is not null && classeIds.Count() > 4)
        {
            // Try to get as many as much of first schedules
            var newSchedules = new HashSet<Schedule>();
            var addedClasses = new HashSet<int>();
            foreach (var item in schedules)
            {
                if (addedClasses.Any(id => id == item.ClassID))
                {
                    newSchedules.Add(item);
                }
                else
                {
                    if (addedClasses.Count < 4)
                    {
                        addedClasses.Add(item.ClassID);
                        newSchedules.Add(item);
                    }
                }
            }
            schedules = newSchedules;
        }
        return false;*/
        #endregion

        var getSchedulesResult = await _scheduleService.GetAllSchedules(1, 100, 100, null, null, preparedDate, preparedDate);
        if (!getSchedulesResult.IsSuccess) return false;

        var schedules = getSchedulesResult.Result;
        if (schedules is null) return false;
        if (schedules.Count() > 4)
        {
            schedules = schedules.Take(4);
        }

        // Count total work amount
        int totalWorkCount = 0;
        var classeIds = schedules.Select(s => s.ClassID).ToHashSet();
        foreach (var item in classeIds)
        {
            var totalStudents = await _studentService.GetStudentsByClassID(item, 1, 100, 50);
            if (totalStudents is not null)
            {
                totalWorkCount = totalWorkCount + totalStudents.Count();
            }
        }

        var sessionResult = _sessionManager.CreatePrepareSchedulesSession(sessionId,
                        preparedDate, schedules.Select(s => s.ScheduleID), totalWorkCount);

        if (!sessionResult)
        {
            return false;
        }

        var session = _sessionManager.GetSessionById(sessionId);
        var websocketEventHandler = _websocketEventManager.GetHandlerByModuleID(session.ModuleId);

        if (websocketEventHandler is not null)
        {
            websocketEventHandler.PrepareSchedules += OnModulePrepareScheduls;
        }
        websocketEventState.SessionId = sessionId;


        var messageSend = new WebsocketMessage
        {
            Event = "PrepareSchedules",
            Data = new
            {
                SessionID = sessionId,
                PrepareDate = preparedDate.ToString("yyyy-MM-dd")
            },
        };
        var jsonPayload = JsonSerializer.Serialize(messageSend);
        var result = await _websocketConnectionManager.SendMesageToModule(jsonPayload, session.ModuleId);
        if (result)
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            if (WaitForPrepareSchedules(cts.Token))
            {
                if (websocketEventHandler is not null)
                {
                    websocketEventHandler.PrepareSchedules -= OnModulePrepareScheduls;
                }

                return true;
            }
        }

        return false;
    }



    private bool WaitForModuleConnecting(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (websocketEventState.ModuleConnected)
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

    private void OnModuleConnectingEventHandler(object? sender, WebsocketEventArgs e)
    {

        if (e.Event == ("Connected " + websocketEventState.SessionId))
        {
            websocketEventState.ModuleConnected = true;
        }
    }


    private bool WaitForPrepareSchedules(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (websocketEventState.PreparedSchedules)
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
    public void OnModulePrepareScheduls(object? sender, WebsocketEventArgs e)
    {
        if (e.Event == ("Prepare schedules " + websocketEventState.SessionId))
        {
            websocketEventState.PreparedSchedules = true;
        }
    }
}

public class WebsocketEventState
{
    public int SessionId { get; set; }
    public bool ModuleConnected { get; set; } = false;
    public bool PreparedSchedules { get; set; } = false;
}
