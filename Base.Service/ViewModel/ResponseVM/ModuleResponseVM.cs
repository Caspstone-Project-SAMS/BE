using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class ModuleResponseVM
{
    public int ModuleID { get; set; }
    public int? Status { get; set; }
    public int? ConnectionStatus { get; set; }
    public int? Mode { get; set; }
    public bool? AutoPrepare { get; set; }
    public TimeOnly? PreparedTime { get; set; }
    public int? AttendanceDurationMinutes { get; set; }
    public int? ConnectionLifeTimeSeconds { get; set; }
    public bool? ConnectionSound { get; set; }
    public int? ConnectionSoundDurationMs { get; set; }
    public bool? AttendanceSound { get; set; }
    public int? AttendanceSoundDurationMs { get; set; }
    public int? Using { get; set; }
    public Employee_ModuleResponseVM? Employee { get; set; }
    public IEnumerable<ModuleActivity_ModuleResponseVM> ModuleActivities { get; set; } = new List<ModuleActivity_ModuleResponseVM>();
}

public class Employee_ModuleResponseVM
{
    public Guid UserId { get; set; }
    public Guid EmployeeID { get; set; }
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
}

public class ModuleActivity_ModuleResponseVM
{
    public int ModuleActivityId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool? IsSuccess { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    public PreparationTask_ModuleResponseVM? PreparationTask { get; set; }
}

public class PreparationTask_ModuleResponseVM
{
    public float? Progress { get; set; }
    public int? PreparedScheduleId { get; set; }
    public IEnumerable<PreparedSchedule_ModuleResponseVM> PreparedSchedules { get; set; } = new List<PreparedSchedule_ModuleResponseVM>();
    public int TotalFingers { get; set; }
    public int UploadedFingers { get; set; }
}

public class PreparedSchedule_ModuleResponseVM
{
    public int? ScheduleId { get; set; }
    public int TotalFingers { get; set; }
    public int UploadedFingers { get; set; }
}