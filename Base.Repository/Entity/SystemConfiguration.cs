using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class SystemConfiguration : ICloneable
{
    [Key]
    public int SystemConfigurationId { get; set; }
    public int RevertableDurationInHours { get; set; } = 24;
    public int ClassCodeMatchRate { get; set; } = 50;
    public int SemesterDurationInDays { get; set; } = 90;
    public int SlotDurationInMins { get; set; } = 135;

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
