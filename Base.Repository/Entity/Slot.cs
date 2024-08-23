using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Slot : AuditableEntity, ICloneable
{
    [Key]
    public int SlotID { get; set; }
    public int SlotNumber { get; set; }
    public int Status { get; set; }
    public int Order { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly Endtime { get; set; }

    public IEnumerable<Schedule> Schedules { get; set; } = new List<Schedule>();

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}