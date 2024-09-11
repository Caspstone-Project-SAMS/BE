using Base.API.Controllers;
using Base.IService.IService;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using Google.Api.Gax;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text.Json;

namespace Base.API.Service;

public class SessionManager
{
    private IList<Session> _sessions = new List<Session>();
    private IList<string> strings = new List<string>();
    private IList<int> OnCompletingSession = new List<int>();

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly WebSocketConnectionManager1 _webSocketConnectionManager;

    public SessionManager(IServiceScopeFactory serviceScopeFactory, WebSocketConnectionManager1 webSocketConnectionManager)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _webSocketConnectionManager = webSocketConnectionManager;
    }


    //===================
    public int CreateSession(int moduleId, Guid userId, int durationinMin)
    {
        var sessionId = _sessions.Count() + 1;
        _sessions.Add(new Session
        {
            SessionId = sessionId,
            UserID = userId,
            ModuleId = moduleId,
            TimeStamp = ServerDateTime.GetVnDateTime(), 
            DurationInMin = durationinMin,
            SessionState = 1
        });
        return sessionId;
    }

    public void DeleteSession(int sessionId)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null) return;
        _sessions.Remove(session);
    }


    //====================
    public bool CreateFingerUpdateSession(int sessionId, Guid studentId, int? fingerTemplateId1, int? fingerTemplateId2)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null) return false;

        var fingerUpdate = new FingerUpdatate
        {
            StudentId = studentId,
            FingerprintTemplateId1 = fingerTemplateId1,
            FingerprintTemplateId2 = fingerTemplateId2
        };

        session.Category = 8;
        session.SessionState = 1;
        session.FingerUpdate = fingerUpdate;

        //var cts = new CancellationTokenSource();
        //cts.CancelAfter(TimeSpan.FromSeconds(240));
        //_ = WaitCancelSession(sessionId, cts.Token);

        return true;
    }

    public bool UpdateFinger(int sessionId, string fingerprintTemplate, int fingerTemplateId, Guid studentId)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null || session.Category != 8 || session.SessionState != 1 || session.FingerUpdate is null || session.FingerUpdate.StudentId != studentId)
        {
            return false;
        }
        if(fingerTemplateId == session.FingerUpdate.FingerprintTemplateId1)
        {
            session.FingerUpdate.FingerprintTemplate1 = fingerprintTemplate;
            session.FingerUpdate.Finger1TimeStamp = ServerDateTime.GetVnDateTime();
            return true;
        }
        else if(fingerTemplateId == session.FingerUpdate.FingerprintTemplateId2)
        {
            session.FingerUpdate.FingerprintTemplate2 = fingerprintTemplate;
            session.FingerUpdate.Finger2TimeStamp = ServerDateTime.GetVnDateTime();
            return true;
        }

        return false;
    }

    public bool CreateFingerRegistrationSession(int sessionId, int fingerRegistrationMode, Guid studentId)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null) return false;

        var fingerRegistration = new FingerRegistration()
        {
            StudentId = studentId,
            FingerRegistrationMode = fingerRegistrationMode
        };

        session.Category = 1;
        session.SessionState = 1;
        session.FingerRegistration = fingerRegistration;

        //var cts = new CancellationTokenSource();
        //cts.CancelAfter(TimeSpan.FromSeconds(240));
        //_ = WaitCancelSession(sessionId, cts.Token);

        return true;
    }

    public bool RegisterFinger(int sessionId, string fingerprintTemplate, int fingerNumber, Guid studentId)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if(session is null || session.Category != 1 || session.SessionState != 1 || session.FingerRegistration is null || session.FingerRegistration.StudentId != studentId)
        {
            return false;
        }
        if(fingerNumber == 1)
        {
            session.FingerRegistration.FingerprintTemplate1 = fingerprintTemplate;
            session.FingerRegistration.Finger1TimeStamp = ServerDateTime.GetVnDateTime();
            return true;
        }
        else if(fingerNumber == 2)
        {
            session.FingerRegistration.FingerprintTemplate2 = fingerprintTemplate;
            session.FingerRegistration.Finger2TimeStamp = ServerDateTime.GetVnDateTime();
            return true;
        }
        else
        {
            return false;
        }
    }



    public async Task<bool> CreatePrepareAScheduleSession(int sessionId, int scheduleId, int totalWorkAmount, int totalFingers)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null) return false;

        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var _scheduleService = serviceScope.ServiceProvider.GetRequiredService<IScheduleService>();
        var existedScheudleTask = _scheduleService.GetByIdForModule(scheduleId);

        var prepareAttendance = new PrepareAttendance()
        {
            ScheduleId = scheduleId,
            Progress = 0,
            TotalWorkAmount = totalWorkAmount,
            TotalFingers = totalFingers
        };

        session.Category = 2;
        session.SessionState = 1;
        session.PrepareAttendance = prepareAttendance;

        await Task.WhenAll(existedScheudleTask);
        var schedule = existedScheudleTask.Result;
        if(schedule is not null)
        {
            session.Title = "Prepare attendance data for class " + (schedule.Class?.ClassCode ?? "***") + " on " + schedule.Date.ToString("dd-MM-yyyy") + " at slot " + (schedule.Slot?.SlotNumber.ToString() ?? "***");
        }

        return true;
    }

    public async Task<bool> CreatePrepareSchedulesSession(int sessionId, DateOnly preparedDate, IEnumerable<Schedule> schedules, int totalWorkAmount, int totalFingers)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null) return false;

        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var _studentService = serviceScope.ServiceProvider.GetRequiredService<IStudentService>();
        var _scheduleService = serviceScope.ServiceProvider.GetRequiredService<IScheduleService>();

        var preparedScheules = new List<PrepareSchedule>();

        foreach (var schedule in schedules)
        {
            var totalStudents = await _studentService.GetStudentsByClassIdv2(1, 100, 50, null, schedule.ClassID);
            var preparedScheulde = new PrepareSchedule
            {
                ScheduleId = schedule.ScheduleID,
                TotalFingers = totalStudents.SelectMany(s => s.FingerprintTemplates).Where(f => f.Status == 1).Count()
            };
            preparedScheules.Add(preparedScheulde);
        }

        var prepareAttendance = new PrepareAttendance()
        {
            PreparedDate = preparedDate,
            Progress = 0,
            TotalWorkAmount = totalWorkAmount,
            PreparedSchedules = preparedScheules,
            TotalFingers = totalFingers
        };

        session.Category = 3;
        session.SessionState = 1;
        session.PrepareAttendance = prepareAttendance;

        var classCodeList = _scheduleService.GetClassCodeList(", ", schedules.Select(s => s.ScheduleID).ToList());
        session.Title = "Prepare attendance data for classes " + (classCodeList ?? "***") + " on " + preparedDate.ToString("dd-MM-yyyy");

        return true;
    }

    public bool UpdateSchedulePreparationResult(int sessionId, int uploadedFingers, int scheduleId)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null || session.SessionState != 1 || (session.Category != 2 && session.Category != 3)) return false;

        if (session.PrepareAttendance is null) return false;

        var preparedScheudle = session.PrepareAttendance.PreparedSchedules.Where(s => s.ScheduleId == scheduleId).FirstOrDefault();

        if (preparedScheudle is null) return false;

        preparedScheudle.UploadedFingers = uploadedFingers;

        return true;
    }

    public bool UpdateSchedulePreparationProgress(int sessionId, int completedWorkAmount)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null || session.SessionState != 1 || (session.Category != 2 && session.Category != 3 )) return false;

        if (session.PrepareAttendance is null) return false;

        session.PrepareAttendance.CompletedWorkAmount = session.PrepareAttendance.CompletedWorkAmount + completedWorkAmount;
        session.PrepareAttendance.Progress = MathF.Round(session.PrepareAttendance.CompletedWorkAmount / session.PrepareAttendance.TotalWorkAmount * 100);

        // Notify to client about changing of progress of the session
        _ = NotifyPreparationProgress(sessionId, session.PrepareAttendance.Progress, session.UserID);

        return true;
    }





    public IEnumerable<Session> GetSessions(Guid? userId, int? state, int? category, int? moduleId, Guid? studentId)
    {
        List<Session> sessions = _sessions.ToList();
        if (userId is not null)
        {
            sessions = sessions.Where(s => s.UserID == userId).ToList();
        }
        if (state is not null && state != 0)
        {
            sessions = sessions.Where(s => s.SessionState == state).ToList();
        }
        if(category is not null && category != 0)
        {
            sessions = sessions.Where(s => s.Category == category).ToList();
        }
        if(moduleId is not null && moduleId != 0)
        {
            sessions = sessions.Where(s => s.ModuleId == moduleId).ToList();
        }
        if(studentId is not null)
        {
            sessions = sessions.Where(s => s.FingerRegistration != null && s.FingerRegistration.StudentId == studentId).ToList();
        }

        return sessions;
    }
    public IEnumerable<Session> GetAllSessions()
    {
        return _sessions.ToList();
    }
    public Session? GetSessionById(int id)
    {
        return _sessions.FirstOrDefault(s => s.SessionId == id);
    }




    //=================================================================
    //=================================================================
    
    // Finish fingerprint registration session, ready to submit
    public void FinishSession(int sessionId)
    {
        var existedSession = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (existedSession is not null)
        {
            existedSession.SessionState = 2;
        }
    }


    // Submit session will apply what session did to the database, for fingerprint registration
    // Only submit when session is finshed
    // After submit successfully, the session will be deleted
    public async Task<ServiceResponseVM> SubmitSession(int sessionId, Guid userId, FingerprintDescription? fingerprintDescription)
    {
        var session = _sessions.FirstOrDefault(s => s.UserID == userId && s.SessionId == sessionId);
        if (session is null)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Submit session failed",
                Errors = new string[1] { "Session not found" }
            };
        }
        if(session.SessionState != 2)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Submit session failed",
                Errors = new string[1] { "Session is not finished" }
            };
        }

        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var fingerprintService = serviceScope.ServiceProvider.GetRequiredService<IFingerprintService>();

        switch (session.Category)
        {
            case 1:
                if (session.FingerRegistration is null)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Title = "Register fingerprint failed",
                        Errors = new string[1] { "Invalid fingerprint information" }
                    };
                }
                if (session.FingerRegistration.FingerprintTemplate1 is null || session.FingerRegistration.FingerprintTemplate2 is null)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Title = "Register fingerprint failed",
                        Errors = new string[1] { "Both 2 fingers are required" }
                    };
                }
                var registerFingerResult = await fingerprintService.RegisterFingerprintTemplate(session.FingerRegistration.StudentId,
                        session.FingerRegistration.FingerprintTemplate1,
                        session.FingerRegistration.Finger1TimeStamp,
                        session.FingerRegistration.FingerprintTemplate2,
                        session.FingerRegistration.Finger2TimeStamp,
                        fingerprintDescription?.fingerprint1Description,
                        fingerprintDescription?.fingerprint2Description);
                if (registerFingerResult.IsSuccess)
                {
                    _sessions.Remove(session);
                }
                return registerFingerResult;

            case 2:
                break;

            case 3:
                break;

            case 8:
                if (session.FingerUpdate is null)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Title = "Update fingerprint failed",
                        Errors = new string[1] { "Invalid fingerprint information" }
                    };
                }
                if (session.FingerUpdate.FingerprintTemplate1 is null && session.FingerUpdate.FingerprintTemplate2 is null)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Title = "Update fingerprint failed",
                        Errors = new string[1] { "Updated fingers not found" }
                    };
                }
                var updateFingerResult = await fingerprintService.UpdateFingerprintTemplate(
                        session.FingerUpdate.StudentId,
                        session.FingerUpdate.FingerprintTemplateId1,
                        session.FingerUpdate.FingerprintTemplate1,
                        session.FingerUpdate.Finger1TimeStamp,
                        session.FingerUpdate.FingerprintTemplateId2,
                        session.FingerUpdate.FingerprintTemplate2,
                        session.FingerUpdate.Finger2TimeStamp,
                        fingerprintDescription?.fingerprint1Description,
                        fingerprintDescription?.fingerprint2Description);
                if (updateFingerResult.IsSuccess)
                {
                    _sessions.Remove(session);
                }
                return updateFingerResult;

            default:
                break;
        }

        return new ServiceResponseVM
        {
            Title = "Submit session failed",
            Errors = new string[1] { "Undefined session" }
        };
    }


    // Call this when user want to cancel session or when module itself cancel session because of there is nothing to do
    // Make the session stop and delete it
    public void CancelSession(int sessionId, Guid userId)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId && s.UserID == userId);
        if (session is null) return;

        // If session is not being used or session is about fingerprint registration, lets delete it
        if (session.Category == 0 || session.Category == 1)
        {
            DeleteSession(sessionId);
        }

        // If session is used for preparing schedule, lets complete it
        if(session.Category == 2 || session.Category == 3 || session.Category == 4)
        {
            _ = CompleteSession(sessionId, false);
        }
    }


    // Call this when ever module got problem, such as lost connection => session cancelled because error
    // Make the session to be cancelled and provide the error that cause it
    public void SessionError(int sessionId, List<string> errors)
    {
        var session = GetSessionById(sessionId);
        if (session is null) return;
        session.SessionState = 2; //End
        var errorList = session.Errors.ToList();
        errorList.AddRange(errors);
        session.Errors = errorList;
    }


    // Call this when session is completed (this should be called by module event, or by server if the event is timed out)
    // Only complete session about schedule preparation (both success and failed cases), setup module
    // This will make a record of module activity and make a notification about that activity
    public async Task<bool> CompleteSession(int sessionId, bool isSucess, int uploadedFingers = 0)
    {
        // Make a notification, activity history of module, and notify the notification to user
        var existedSession = GetSessionById(sessionId);
        if (existedSession is null) return false;


        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var scheduleService = serviceScope.ServiceProvider.GetRequiredService<IScheduleService>();


        // Create activity
        var newActivity = new ActivityHistoryVM
        {
            UserId = existedSession.UserID,
            StartTime = existedSession.TimeStamp,
            EndTime = ServerDateTime.GetVnDateTime(),
            IsSuccess = isSucess,
            ModuleID = existedSession.ModuleId
        };
        string description = "";
        string title = "";
        // Make a title and description for activity
        if (existedSession.Category == 2)
        {
            title = "Schedule preparation";
            var schedule = await scheduleService.GetById(existedSession.PrepareAttendance?.ScheduleId ?? 0);
            if (schedule is not null)
            {
                description = "Prepare attendance data for class " 
                    + (schedule.Class?.ClassCode ?? "***")
                    + " at " 
                    + (schedule.Slot?.StartTime.ToString("HH:mm:ss") ?? "***")
                    + " - " + (schedule.Slot?.Endtime.ToString("HH:mm:ss") ?? "***")
                    + " on " + (schedule.Date.ToString("dd-MM-yyyy") ?? "***");
            }
        }
        else if (existedSession.Category == 3)
        {
            title = "Schedules preparation";
            var preparedDate = existedSession.PrepareAttendance?.PreparedDate;
            var classCodeList = scheduleService.GetClassCodeList(", ", existedSession.PrepareAttendance?.PreparedSchedules.Select(s => s.ScheduleId).ToList());
            description = "Prepare attendance data for classes "
                    + (classCodeList ?? "***")
                    + " on ";
            if(preparedDate is null)
            {
                description = description + "***";
            }
            else
            {
                description = description + preparedDate.Value.ToString("dd-MM-yyyy");
            }
        }
        else if (existedSession.Category == 4)
        {
            title = "Setup";
        }
        // Make a description for activity
        if (!isSucess)
        {
            newActivity.Errors = existedSession.Errors;
            description = description + " failed";
        }
        else
        {
            description = description + " successfully";
        }
        newActivity.Description = description;
        newActivity.Title = title;
        if (existedSession.Category == 2 || existedSession.Category == 3)
        {
            var preparedSchedules = new List<PreparedScheduleVM>();
            if(existedSession.PrepareAttendance?.PreparedSchedules is not null)
            {
                foreach (var schedule in existedSession.PrepareAttendance.PreparedSchedules)
                {
                    preparedSchedules.Add(new PreparedScheduleVM
                    {
                        ScheduleId = schedule.ScheduleId,
                        TotalFingers = schedule.TotalFingers,
                        UploadedFingers = schedule.UploadedFingers
                    });
                }
            }
            
            var preparationTask = new PreparationTaskVM
            {
                Progress = existedSession.PrepareAttendance?.Progress ?? 0,
                PreparedScheduleId = existedSession.PrepareAttendance?.ScheduleId,
                PreparedSchedules = preparedSchedules,
                PreparedDate = existedSession.PrepareAttendance?.PreparedDate,
                UploadedFingers = uploadedFingers,
                TotalFingers = existedSession.PrepareAttendance?.TotalFingers ?? 0
            };
            newActivity.PreparationTaskVM = preparationTask;
        }
        var activityHistoryService = serviceScope.ServiceProvider.GetRequiredService<IModuleActivityService>();
        var createNewActivityResult = await activityHistoryService.Create(newActivity);
        if (!createNewActivityResult.IsSuccess)
        {
            return false;
        }
        DeleteSession(sessionId);


        // Create notification about the activity
        var notificationTypeService = serviceScope.ServiceProvider.GetRequiredService<INotificationTypeService>();
        var notificationService = serviceScope.ServiceProvider.GetRequiredService<INotificationService>();
        var informationType = (await notificationTypeService.GetAll(1, 1, 1, "Information", null)).Result?.FirstOrDefault();
        var errorType = (await notificationTypeService.GetAll(1, 1, 1, "Error", null)).Result?.FirstOrDefault();
        if(informationType is null)
        {
            var newNotificationType = new NotificationTypeVM
            {
                TypeName = "Information",
                TypeDescription = "Used to informs the details of something"
            };
            informationType = (await notificationTypeService.Create(newNotificationType)).Result;
        }
        if(errorType is null)
        {
            errorType = ( await notificationTypeService.Create(new NotificationTypeVM
            {
                TypeName = "Error",
                TypeDescription = "Used to inform user there is something bad happened"
            })).Result;
        }
        var newNotification = new NotificationVM
        {
            Title = title,
            Description = description,
            Read = false,
            UserID = existedSession.UserID,
        };
        if (isSucess) newNotification.NotificationTypeID = informationType!.NotificationTypeID;
        else newNotification.NotificationTypeID = errorType!.NotificationTypeID;
        newNotification.ModuleActivityId = createNewActivityResult.Result?.ModuleActivityId;
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
            _ = _webSocketConnectionManager.SendMessageToRootClient(jsonPayload, existedSession.UserID);
        }


        // After all, then delete the session
        DeleteSession(sessionId);

        return true;
    }


    // For test purpose
    public void AddString(string text)
    {
        strings.Add(text);
    }
    public IEnumerable<string> GetAllString()
    {
        return strings;
    }
    public void DeleteAllString()
    {
        strings.Clear();
    }
    /*public void CreateNewSessionForTest()
    {
        var prepareAttendance = new PrepareAttendance
        {
            ScheduleId = 47,
            Progress = 100,
            PreparedDate = null,
            ScheduleIds = Enumerable.Empty<int>()
        };
        var sessionId = _sessions.Count() + 1;
        _sessions.Add(new Session
        {
            SessionId = sessionId,
            UserID = new Guid("A829C0B5-78DC-4194-A424-08DC8640E68A"),
            ModuleId = 5,
            TimeStamp = ServerDateTime.GetVnDateTime(),
            DurationInMin = 1,
            SessionState = 2,
            Category = 2,
            PrepareAttendance = prepareAttendance
        });
    }*/


    public async Task NotifyPreparationProgress(int sessionId, float progress, Guid userId)
    {
        var messageSend = new WebsocketMessage
        {
            Event = "PreparationProgress",
            Data = new
            {
                SessionId = sessionId,
                Progress = progress,
            }
        };
        var jsonPayload = JsonSerializer.Serialize(messageSend);
        _ = _webSocketConnectionManager.SendMessageToClient(jsonPayload, userId);
        await _webSocketConnectionManager.SendMessageToRootClient(jsonPayload, userId);
    }

    private async Task WaitCancelSession(int sessionId)
    {
        await Task.Delay(TimeSpan.FromSeconds(240));

        DeleteSession(sessionId);

        // Notify module and client about the session cancelled
    }
}

public class Session
{
    public int SessionId { get; set; }
    public Guid UserID { get; set; }
    public int Category { get; set; }
    public DateTime TimeStamp { get; set; }
    public int SessionState { get; set; }
    public int DurationInMin { get; set; }
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public FingerRegistration? FingerRegistration { get; set; }
    public FingerUpdatate? FingerUpdate { get; set; }
    public PrepareAttendance? PrepareAttendance { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}

public class FingerRegistration
{
    public Guid StudentId { get; set; }
    public int FingerRegistrationMode { get; set; }
    public string? FingerprintTemplate1 { get; set; }
    public DateTime? Finger1TimeStamp { get; set; }
    public string? FingerprintTemplate2 { get; set; }
    public DateTime? Finger2TimeStamp { get; set; }
}

public class FingerUpdatate
{
    public Guid StudentId { get; set; }
    public int? FingerprintTemplateId1 { get; set; }
    public string? FingerprintTemplate1 { get; set; }
    public DateTime? Finger1TimeStamp { get; set; }
    public int? FingerprintTemplateId2 { get; set; }
    public string? FingerprintTemplate2 { get; set; }
    public DateTime? Finger2TimeStamp { get; set; }

}

public class PrepareAttendance
{
    public IEnumerable<PrepareSchedule> PreparedSchedules { get; set; } = new List<PrepareSchedule>();
    public DateOnly? PreparedDate { get; set; }
    public int? ScheduleId { get; set; }
    public float Progress { get; set; }
    public float TotalWorkAmount { get; set; }
    public float CompletedWorkAmount { get; set; }
    public int TotalFingers { get; set; }
}

public class PrepareSchedule
{
    public int ScheduleId { get; set; }
    public int TotalFingers { get; set; }
    public int UploadedFingers { get; set; }
}


// Session state: 1 - onGoing, 2 - End
// FingerScanState: 1 - finger1, 2 - finger2, 3 - both fingers
// FingerRegistrationMode: 1 - finger1, 2 - finger2, 3 - both fingers
// Category: 1 - FingerRegistration, 2 - Prepare a schedule, 3 - Prepare schedules of a day, 4 - Setup

// Session về kết nối module, đăng ký vân tay, chuẩn bị dữ liệu điểm danh