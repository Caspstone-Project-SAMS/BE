using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class SlotResponseVM
{
    public int SlotID { get; set; }
    public int? SlotNumber { get; set; }
    public int? Status { get; set; }
    public int? Order { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? Endtime { get; set; }
}
