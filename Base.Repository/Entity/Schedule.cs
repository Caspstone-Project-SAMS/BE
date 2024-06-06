using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Schedule : AuditableEntity
{
    [Key]
    public int ScheduleID { get; set; }
    public DateOnly Date { get; set; }
    public int DateOfWeek { get; set; }
    public int ScheduleStatus { get; set; }

    public int SlotID { get; set; }
    public Slot? Slot { get; set; }

    public int ClassID { get; set; }
    public Class? Class { get; set; }

    public SubstituteTeaching? SubstituteTeaching { get; set; }

    public IEnumerable<Attendance> Attendances { get; set; } = new List<Attendance>();

}