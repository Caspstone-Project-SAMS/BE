﻿using Base.API.Common;
using Base.API.Controllers;
using Base.IService.IService;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.Common;
using Base.Service.IService;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Base.Service.ViewModel.RequestVM;
using Base.Service.Service;



namespace Base.API.Service;

public class HangfireService : IHangfireService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly WebSocketConnectionManager1 _websocketConnectionManager;
    private readonly SessionManager _sessionManager;
    private readonly ICurrentUserService _currentUserService;
    private readonly WebsocketEventManager _websocketEventManager;
    private readonly WebsocketEventState websocketEventState = new WebsocketEventState();
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public HangfireService(IBackgroundJobClient backgroundJobClient,
                           IRecurringJobManager recurringJobManager,
                           WebSocketConnectionManager1 websocketConnectionManager,
                           SessionManager sessionManager,
                           ICurrentUserService currentUserService,
                           WebsocketEventManager websocketEventManager,
                           IServiceScopeFactory serviceScopeFactory)
    {
        _backgroundJobClient = backgroundJobClient;
        _recurringJobManager = recurringJobManager;
        _websocketConnectionManager = websocketConnectionManager;
        _sessionManager = sessionManager;
        _currentUserService = currentUserService;
        _websocketEventManager = websocketEventManager;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task SlotProgress()
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var _slotService = serviceScope.ServiceProvider.GetRequiredService<ISlotService>();

        var slots = await _slotService.Get();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.4 * 2))
        };
        Parallel.ForEach(slots, parallelOptions, (slot, state) =>
        {
            _recurringJobManager.AddOrUpdate(
                $"Set slot progress_SlotId-{slot.SlotID}_Start",
                () => SetSlotStart(slot.SlotID, null),
                ConvertToCronExpression(slot.StartTime),
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                }
            );
            _recurringJobManager.AddOrUpdate(
                $"Set slot progress_SlotId-{slot.SlotID}_End",
                () => SetSlotEnd(slot.SlotID, null),
                ConvertToCronExpression(slot.Endtime),
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                }
            );
        });
    }

    public void SetASlotProgress(int slotId, TimeOnly startTime, TimeOnly endTime)
    {
        _recurringJobManager.AddOrUpdate(
                $"Set slot progress_SlotId-{slotId}_Start",
                () => SetSlotStart(slotId, null),
                ConvertToCronExpression(startTime),
                new RecurringJobOptions
                {
                    TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
                }
            );
        _recurringJobManager.AddOrUpdate(
            $"Set slot progress_SlotId-{slotId}_End",
            () => SetSlotEnd(slotId, null),
            ConvertToCronExpression(endTime),
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            }
        );
    }

    public void CheckAbsenceRoutine()
    {
        _recurringJobManager.AddOrUpdate(
            "Check Absence Routine",
            () => SendAbsenceEmails(),
            "00 19 * * *",
            new RecurringJobOptions
            {
                TimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            }
        );
    }

    public async Task SendAbsenceEmails()
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var mailService = serviceScope.ServiceProvider.GetRequiredService<IMailService>();
        var _unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var entities = await _unitOfWork.StudentClassRepository.GetStudentClassInfoAsync();

        foreach (var entity in entities)
        {
            var emailMessage = new Message
            {
                To = entity.Email,
                Subject = "Your Absence Report",
                Content = $@"<html>
                    <body>
                    <p>Dear Student,</p>
                    <p>Here is your attendance report:</p>
                    <ul>
                        <li><strong>Student Code:</strong> {entity.StudentCode}</li>
                        <li><strong>Class Code:</strong> {entity.ClassCode}</li>
                        <li><strong>Absence Percentage:</strong> {entity.AbsencePercentage}%</li>
                    </ul>
                    <p>Best regards,<br>SAMS Team</p>
                    </body>
                    </html>"
            };
            await mailService.SendMailAsync(emailMessage);
            var update = _unitOfWork.StudentClassRepository.Get("StudentClass",s => s.StudentID.Equals(entity.ID) && s.ClassID == entity.ClassID && !s.IsDeleted).FirstOrDefault();

            if (update is null)
            {
                continue;
            }
            update.IsSendEmail = true;
            _unitOfWork.StudentClassRepository.Update(update);
        }
       await _unitOfWork.SaveChangesAsync();
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



    public async Task SetSlotStart(int slotId, DateOnly? date)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var _unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        DateOnly currentDate;
        if (date is not null)
        {
            currentDate = date.Value;
        }
        else
        {
            currentDate = DateOnly.FromDateTime(ServerDateTime.GetVnDateTime());
        }
        
        var schedules = _unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Date == currentDate && s.SlotID == slotId)
            .ToList();

        foreach(var schedule in schedules)
        {
            schedule.ScheduleStatus = 2;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task SetSlotEnd(int slotId, DateOnly? date)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var _unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        DateOnly currentDate;
        if (date is not null)
        {
            currentDate = date.Value;
        }
        else
        {
            currentDate = DateOnly.FromDateTime(ServerDateTime.GetVnDateTime());
        }

        var schedules = _unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Date == currentDate && s.SlotID == slotId)
            .ToList();

        foreach (var schedule in schedules)
        {
            schedule.ScheduleStatus = 3;
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<string> SetupPreparationForModule(DateOnly date, int moduleId)
    {
        var sessionId = await ConnectModule(moduleId);
        if (sessionId is null)
        {
            // Should record failed case
            await RecordFailedModuleActivity(date, moduleId);
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

    public async Task<int?> ConnectModule(int moduleId)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var moduleService = serviceScope.ServiceProvider.GetRequiredService<IModuleService>();

        var websocketEventHandler = _websocketEventManager.GetHandlerByModuleID(moduleId);

        var module = await moduleService.GetById(moduleId);
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
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var _scheduleService = serviceScope.ServiceProvider.GetRequiredService<IScheduleService>();
        var _studentService = serviceScope.ServiceProvider.GetRequiredService<IStudentService>();

        var session = _sessionManager.GetSessionById(sessionId);
        if(session is null) return false;

        var getSchedulesResult = await _scheduleService.GetAllSchedules(1, 100, 100, session.UserID, null, preparedDate, preparedDate);
        if (!getSchedulesResult.IsSuccess) return false;

        var schedules = getSchedulesResult.Result;
        if (schedules is null) return false;
        if (schedules.Count() > 4)
        {
            schedules = schedules.Take(4);
        }

        // Count total work amount
        int totalWorkCount = 0;
        int totalFingers = 0;
        var classeIds = schedules.Select(s => s.ClassID);
        var addedClassId = new List<int>();
        foreach (var item in classeIds)
        {
            var totalStudents = await _studentService.GetStudentsByClassIdv2(1, 100, 50, null, item);
            if (totalStudents is not null)
            {
                totalWorkCount = totalWorkCount + totalStudents.Count();
                if (!addedClassId.Contains(item))
                {
                    totalFingers = totalFingers + totalStudents.SelectMany(s => s.FingerprintTemplates).Where(f => f.Status == 1).Count();
                    addedClassId.Add(item);
                }
            }
        }

        var sessionResult = _sessionManager.CreatePrepareSchedulesSession(sessionId,
                        preparedDate, schedules.Select(s => s.ScheduleID), totalWorkCount, totalFingers);

        if (!sessionResult)
        {
            return false;
        }

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

                // Notify the action is started
                _ = _sessionManager.NotifyPreparationProgress(sessionId, 0, session.UserID);

                return true;
            }
        }

        return false;
    }

    private async Task RecordFailedModuleActivity(DateOnly date, int moduleId)
    {
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var _unitOfWork = serviceScope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var _scheduleService = serviceScope.ServiceProvider.GetRequiredService<IScheduleService>();
        var _moduleActivityService = serviceScope.ServiceProvider.GetRequiredService<IModuleActivityService>();

        var existedModule = _unitOfWork.ModuleRepository
            .Get(m => !m.IsDeleted && m.ModuleID == moduleId,
            new Expression<Func<Module, object?>>[]
            {
                m => m.Employee!.User
            })
            .AsNoTracking()
            .FirstOrDefault();
        if (existedModule is null) return;

        var scheduleIds = _unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Date == date && s.Class!.LecturerID == existedModule.Employee!.User!.Id)
            .AsNoTracking()
            .Select(s => s.ScheduleID)
            .ToList();

        var classCodeList = _scheduleService.GetClassCodeList(", ", scheduleIds);

        // Create activity
        var preparationTask = new PreparationTaskVM
        {
            Progress = 0,
            PreparedScheduleIds = Enumerable.Empty<int>(),
            PreparedDate = date,
            UploadedFingers = 0,
            TotalFingers = 0
        };
        var newActivity = new ActivityHistoryVM
        {
            UserId = existedModule.Employee?.User?.Id,
            StartTime = ServerDateTime.GetVnDateTime(),
            EndTime = ServerDateTime.GetVnDateTime(),
            IsSuccess = false,
            Errors = new string[1] { "Module is not being connected" },
            ModuleID = moduleId,
            Title = "Schedules preparation",
            Description = "Prepare attendance data for classes " + (classCodeList ?? "***")
                    + " on " + date.ToString("yyyy-MM-dd") + " failed",
            PreparationTaskVM = preparationTask
        };

        var createNewActivityResult = await _moduleActivityService.Create(newActivity);

        if (createNewActivityResult.IsSuccess)
        {
            // Create notification about the activity
            var notificationTypeService = serviceScope.ServiceProvider.GetRequiredService<INotificationTypeService>();
            var notificationService = serviceScope.ServiceProvider.GetRequiredService<INotificationService>();
            var errorType = (await notificationTypeService.GetAll(1, 1, 1, "Error", null)).Result?.FirstOrDefault();
            if (errorType is null)
            {
                errorType = (await notificationTypeService.Create(new NotificationTypeVM
                {
                    TypeName = "Error",
                    TypeDescription = "Used to inform user there is something bad happened"
                })).Result;
            }
            var newNotification = new NotificationVM
            {
                Title = newActivity.Title,
                Description = newActivity.Description,
                Read = false,
                UserID = existedModule.Employee?.User?.Id ?? Guid.Empty,
            };
            newNotification.NotificationTypeID = errorType!.NotificationTypeID;
            var notificationResult = await notificationService.Create(newNotification);

            // Notify the notification
            if (notificationResult.IsSuccess)
            {
                var messageSend = new WebsocketMessage
                {
                    Event = "NewNotification",
                    Data = new
                    {
                        NotificationId = notificationResult.Result!.NotificationID
                    }
                };
                var jsonPayload = JsonSerializer.Serialize(messageSend);
                await _websocketConnectionManager.SendMessageToClient(jsonPayload, existedModule.Employee?.User?.Id ?? Guid.Empty);
            }
        }
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
