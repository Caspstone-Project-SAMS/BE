using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Slot : AuditableEntity
{
    [Key]
    public int SlotID { get; set; }
    public int SlotNumber { get; set; }
    public int Status { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly Endtime { get; set; }

    public IEnumerable<ScheduleTable> ScheduleTables { get; set; } = new List<ScheduleTable>();
}