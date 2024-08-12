using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;
using Base.Repository.Identity;

namespace Base.Repository.Entity;

public class Attendance : AuditableEntity
{
    [Key]
    public int AttendanceID { get; set; }
    public int AttendanceStatus { get; set; }
    public DateTime? AttendanceTime { get; set; }
    public DateTime? ScannedTime { get; set; }
    public string? Comments { get; set; }

    public int ScheduleID { get; set; }
    public Schedule? Schedule { get; set; }

    public Guid StudentID { get; set; }
    public User? Student { get; set; }
}