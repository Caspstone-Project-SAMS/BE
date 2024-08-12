using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Module : AuditableEntity
{
    [Key]
    public int ModuleID { get; set; }
    public int Status { get; set; }
    public int Mode { get; set; }
    public string Key { get; set; } = string.Empty;


    // Sound of buzzer
    public bool ConnectionSound { get; set; } = true;
    public int ConnectionSoundDurationMs { get; set; } = 500;
    public bool AttendanceSound { get; set; } = false;
    public int AttendanceSoundDurationMs { get; set; } = 500;


    // Module activities
    public int ConnectionLifetimeMs { get; set; } = 100;


    // Attendance
    public int AttendanceGracePeriodMinutes { get; set; } = 15;
    // Data Preparation
    public bool AutoPrepare { get; set; } = false;
    public TimeOnly? PreparedTime { get; set; }
    public int? PreparedMinBeforeSlot { get; set; }


    public Guid EmployeeID { get; set; }
    public Employee? Employee { get; set; }

    // A module have activities
    public IEnumerable<ModuleActivity> ModuleActivities { get; set; } = new List<ModuleActivity>();
}