using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Http;

namespace Base.API.Service;

public class SessionManager
{
    private IList<Session> _sessions = new List<Session>();
    private IList<string> strings = new List<string>();

    public int CreateSession(int moduleId, Guid userId)
    {
        var sessionId = _sessions.Count() + 1;
        _sessions.Add(new Session
        {
            UserID = userId,
            ModuleId = moduleId,
            TimeStamp = DateTime.Now,
        });
        return sessionId;
    }

    public void DeleteSession(int sessionId)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId);
        if (session is null) return;
        _sessions.Remove(session);
    }

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

    public void CancelSession(int sessionId, Guid userId)
    {
        var session = _sessions.FirstOrDefault(s => s.SessionId == sessionId && s.UserID == userId);
        if (session is null) return;
        session.SessionState = 2;
    }



    public IEnumerable<Session> GetSessions(Guid? userId, int? state, int? category)
    {
        var sessions = _sessions;
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

        return sessions;
    }

    public Session? GetSessionById(int id)
    {
        return _sessions.FirstOrDefault(s => s.SessionId == id);
    }


    public void SessionError(int sessionId, List<string> errors)
    {
        var session = GetSessionById(sessionId);
        if (session is null) return;
        session.SessionState = 2; //End
        session.Errors = errors;
    }


    public void AddString(string text)
    {
        strings.Add(text);
    }
    public IEnumerable<string> GetAllString()
    {
        return strings;
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


// Session state: 1 - onGoing, 2 - End
// FingerScanState: 1 - finger1, 2 - finger2, 3 - both fingers
// FingerRegistrationMode: 1 - finger1, 2 - finger2, 3 - both fingers
// Category: 1 - FingerRegistration, 2 - StartAttendance, 3 - StopAttedance