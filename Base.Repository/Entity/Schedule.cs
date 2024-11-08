using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Schedule : AuditableEntity, ICloneable
{
    [Key]
    public int ScheduleID { get; set; }
    public DateOnly Date { get; set; }
    public int DateOfWeek { get; set; }
    public int ScheduleStatus { get; set; } = 1;
    [NotMapped]
    public string? AttendStudent { get; set; }
    public int Attended { get; set; } = 1;

    public int SlotID { get; set; }
    public Slot? Slot { get; set; }

    public int ClassID { get; set; }
    public Class? Class { get; set; }

    public int? RoomID { get; set; }
    public Room? Room { get; set; }

    public int? ImportSchedulesRecordID { get; set; }
    public ImportSchedulesRecord? ImportSchedulesRecord { get; set; }

    public SubstituteTeaching? SubstituteTeaching { get; set; }

    public IEnumerable<Attendance> Attendances { get; set; } = new List<Attendance>();

    public IEnumerable<PreparedSchedule> PreparedSchedules { get; set; } = new List<PreparedSchedule>();

    public IEnumerable<Notification> Notifications { get; set; } = new List<Notification>();

    public object Clone()
    {
        return this.MemberwiseClone();
    }

    public string? GetAttendanceRate()
    {
        var attended = this.Attendances.Where(a => a.AttendanceStatus == 1).Count();
        return $"{attended}/{this.Attendances.Count()}";
    }
}