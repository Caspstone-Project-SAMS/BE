using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class SlotTypeResponseVM
{
    public int SlotTypeID { get; set; }
    public string? TypeName { get; set; }
    public string? Description { get; set; }
    public int? Status { get; set; }
    public int? SessionCount { get; set; }
    public IEnumerable<Slot_SlotTypeResponseVM> Slots { get; set; } = new List<Slot_SlotTypeResponseVM>();
}

public class Slot_SlotTypeResponseVM
{
    public int SlotID { get; set; }
    public int? SlotNumber { get; set; }
    public int? Status { get; set; }
    public int? Order { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? Endtime { get; set; }
}
