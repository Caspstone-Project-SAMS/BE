using DocumentFormat.OpenXml.InkML;

namespace Base.API.Service;

public class SessionManager
{
    private IList<Session> _fingerRegistrationSessions = new List<Session>();

    public int CreateFingerRegistrationSession(int fingerRegistrationMode)
    {
        var fingerRegistration = new FingerRegistration()
        {
            FingerRegistrationMode = fingerRegistrationMode
        };

        var sessionId = _fingerRegistrationSessions.Count() + 1;

        var session = new Session()
        {
            SessionId = sessionId,
            Category = 1,
            TimeStamp = DateTime.Now,
            SessionState = 1,
            DurationInMin = 10,
            FingerRegistration = fingerRegistration
        };

        _fingerRegistrationSessions.Add(session);

        return sessionId;
    }

    public bool RegisterFinger(int sessionId, string fingerprintTemplate, int fingerNumber)
    {
        var session = _fingerRegistrationSessions.FirstOrDefault(s => s.SessionId == sessionId);
        if(session is null || session.Category != 1 || session.SessionState != 1 || session.FingerRegistration is null)
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

    public Session? GetSessionById(int id)
    {
        return _fingerRegistrationSessions.FirstOrDefault(s => s.SessionId == id);
    }
}

public class Session
{
    public int SessionId { get; set; }
    public int Category { get; set; }
    public DateTime TimeStamp { get; set; }
    public int SessionState { get; set; }
    public int DurationInMin { get; set; }
    public FingerRegistration? FingerRegistration { get; set; }
}

public class FingerRegistration
{
    public int FingerRegistrationMode { get; set; }
    public string FingerprintTemplate1 { get; set; } = string.Empty;
    public DateTime? Finger1TimeStamp { get; set; }
    public string FingerprintTemplate2 { get; set; } = string.Empty;
    public DateTime? Finger2TimeStamp { get; set; }
}


// Session state: 1 - onGoing, 2 - End, 3 - Error, 4 - disruption
// FingerScanState: 1 - finger1, 2 - finger2, 3 - both fingers
// FingerRegistrationMode: 1 - finger1, 2 - finger2, 3 - both fingers
// Category: 1 - FingerRegistration, 2 - StartAttendance, 3 - StopAttedance