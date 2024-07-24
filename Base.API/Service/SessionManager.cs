using Base.Service.Common;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;

namespace Base.API.Service;

public class SessionManager
{
    private IList<Session> _sessions = new List<Session>();
    private IList<string> strings = new List<string>();

    private readonly IServiceScopeFactory _serviceScopeFactory;

    public SessionManager(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
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
    public bool CreateFingerRegistrationSession(int sessionId, int fingerRegistrationMode, Guid userId, Guid studentId)
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

        return true;
    }

    public bool RegisterFinger(int sessionId, string fingerprintTemplate, int fingerNumber, Guid studentId)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId && s.Category == 1);
        if(session is null || session.Category != 1 || session.SessionState != 1 || session.FingerRegistration is null || session.FingerRegistration.StudentId != studentId)
        {
            return false;
        }
        if(fingerNumber == 1)
        {
            session.FingerRegistration.FingerprintTemplate1 = fingerprintTemplate;
            session.FingerRegistration.Finger1TimeStamp = DateTime.Now;
        }
        if(fingerNumber == 2)
        {
            session.FingerRegistration.FingerprintTemplate2 = fingerprintTemplate;
            session.FingerRegistration.Finger2TimeStamp = DateTime.Now;
        }
        return true;
    }


    public bool CreatePrepareAScheduleSession(int sessionId, int scheduleId, int totalWorkAmount)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null) return false;

        var prepareAttendance = new PrepareAttendance()
        {
            ScheduleId = scheduleId,
            Progress = 0,
            TotalWorkAmount = totalWorkAmount
        };

        session.Category = 2;
        session.SessionState = 1;
        session.PrepareAttendance = prepareAttendance;

        return true;
    }

    public bool UpdateSchedulePreparationProgress(int sessionId, int completedWorkAmount)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null || session.SessionState != 1 || session.Category != 2) return false;

        if (session.PrepareAttendance is null) return false;

        session.PrepareAttendance.CompletedWorkAmount = session.PrepareAttendance.CompletedWorkAmount + completedWorkAmount;
        session.PrepareAttendance.Progress = MathF.Round((session.PrepareAttendance.CompletedWorkAmount / session.PrepareAttendance.TotalWorkAmount) * 100);
        return true;
    }





    public IEnumerable<Session> GetSessions(Guid? userId, int? state, int? category, int? moduleId, Guid? studentId)
    {
        List<Session> sessions = _sessions.ToList();
        if (userId is not null)
        {
            sessions = sessions.Where(s => s.UserID == userId).ToList();
        }
        if (state is not null)
        {
            sessions = sessions.Where(s => s.SessionState == state).ToList();
        }
        if(category is not null)
        {
            sessions = sessions.Where(s => s.Category == category).ToList();
        }
        if(moduleId is not null)
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


    // Submit session will apply what session did to the database, for fingerprint registration
    public async Task<ServiceResponseVM> SubmitSession(int sessionId, Guid userId)
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
                        Title = "Submit session failed",
                        Errors = new string[1] { "Invalid fingerprint information" }
                    };
                }
                var registerFingerResult = await fingerprintService.RegisterFingerprintTemplate(session.FingerRegistration.StudentId,
                        session.FingerRegistration.FingerprintTemplate1,
                        session.FingerRegistration.Finger1TimeStamp,
                        session.FingerRegistration.FingerprintTemplate2,
                        session.FingerRegistration.Finger2TimeStamp);
                if (registerFingerResult.IsSuccess)
                {
                    _sessions.Remove(session);
                }
                return registerFingerResult;

            case 2:
                break;

            case 3:
                break;

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
    // Make the session stop
    public void CancelSession(int sessionId, Guid userId)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId && s.UserID == userId);
        if (session is null) return;
        session.SessionState = 2;
    }


    // Call this when ever module got problem, such as lost connection => session cancelled because error
    // Make the session to be cancelled and provide the error that cause it
    public void SessionError(int sessionId, List<string> errors)
    {
        var session = GetSessionById(sessionId);
        if (session is null) return;
        session.SessionState = 2; //End
        session.Errors = errors;
    }


    // Call this when session is completed (this should be called by module event, or by server if the event is timed out)
    // Only complete session about schedule preparation (both success and failed cases), setup module
    // This will make a record of module activity and make a notification about that activity
    public async Task<bool> CompleteSession(int sessionId, bool isSucess)
    {
        // Make a notification, activity history of module, and notify the notification to user
        var existedSession = GetSessionById(sessionId);
        if (existedSession is null) return false;

        // Create activity
        var newActivity = new ActivityHistoryVM
        {
            UserId = existedSession.UserID,
            StartTime = existedSession.TimeStamp,
            EndTime = ServerDateTime.GetVnDateTime(),
            IsSuccess = isSucess,
            ModuleID = existedSession.ModuleId
        };
        if (!isSucess)
        {
            newActivity.Errors = existedSession.Errors;
        }
        if(existedSession.Category == 2 || existedSession.Category == 3)
        {
            var preparationTask = new PreparationTaskVM
            {
                Progress = existedSession.PrepareAttendance?.Progress ?? 0,
                PreparedScheduleId = existedSession.PrepareAttendance?.ScheduleId,
                PreparedScheduleIds = existedSession.PrepareAttendance?.ScheduleIds ?? Enumerable.Empty<int>(),
                PreparedDate = existedSession.PrepareAttendance?.PreparedDate
            };
            newActivity.PreparationTaskVM = preparationTask;
            if(existedSession.Category == 2)
            {
                newActivity.Title = "Prepare a schedule";
            }
            else
            {
                newActivity.Title = "Prepare schedules for a day";
            }
        }
        using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();
        var activityHistoryService = serviceScope.ServiceProvider.GetRequiredService<IActivityHistoryService>();
        var createNewActivityResult = await activityHistoryService.Create(newActivity);
        if (!createNewActivityResult.IsSuccess)
        {
            return false;
        }

        // Create notification about the activity


        // Notify the notification

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
    public FingerRegistration? FingerRegistration { get; set; }
    public PrepareAttendance? PrepareAttendance { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();
}

public class FingerRegistration
{
    public Guid StudentId { get; set; }
    public int FingerRegistrationMode { get; set; }
    public string FingerprintTemplate1 { get; set; } = string.Empty;
    public DateTime? Finger1TimeStamp { get; set; }
    public string FingerprintTemplate2 { get; set; } = string.Empty;
    public DateTime? Finger2TimeStamp { get; set; }
}

public class PrepareAttendance
{
    public IEnumerable<int> ScheduleIds { get; set; } = Enumerable.Empty<int>();
    public DateOnly? PreparedDate { get; set; }
    public int ScheduleId { get; set; }
    public float Progress { get; set; }
    public int TotalWorkAmount { get; set; }
    public int CompletedWorkAmount { get; set; }
}


// Session state: 1 - onGoing, 2 - End
// FingerScanState: 1 - finger1, 2 - finger2, 3 - both fingers
// FingerRegistrationMode: 1 - finger1, 2 - finger2, 3 - both fingers
// Category: 1 - FingerRegistration, 2 - Prepare a schedule, 3 - Prepare schedules of a day, 4 - Setup

// Session về kết nối module, đăng ký vân tay, chuẩn bị dữ liệu điểm danh