using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM;

public class SystemConfigurationVM
{
    public int? RevertableDurationInHours { get; set; }
    public int? ClassCodeMatchRate { get; set; }
    public int? SemesterDurationInDays { get; set; }
    public int? SlotDurationInMins { get; set; }
}
