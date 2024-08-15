using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class SystemConfiguration
{
    [Key]
    public int SystemConfigurationId { get; set; }
    public int RevertableDurationInHours { get; set; } = 24;
    public int ClassCodeMatchRate { get; set; } = 50;
}
