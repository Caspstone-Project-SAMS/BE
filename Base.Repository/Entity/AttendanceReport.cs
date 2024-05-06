using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class AttendanceReport : AuditableEntity
{
    [Key]
    public int AttendanceReportID { get; set; }
    public int AttendanceStatus { get; set; }
    public string Comments { get; set; } = string.Empty;

    public int ScheduleID { get; set; }
    public ScheduleTable? ScheduleTable { get; set; }

    public Guid StudentID { get; set; }
    public Student? Student { get; set; }
}