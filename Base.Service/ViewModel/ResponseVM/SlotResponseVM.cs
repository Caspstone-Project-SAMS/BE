﻿using System;
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
    public SlotType_SlotResponseVM? SlotType { get; set; }
}

public class SlotType_SlotResponseVM
{
    public int SlotTypeID { get; set; }
    public string? TypeName { get; set; }
    public string? Description { get; set; }
    public int? Status { get; set; }
    public int? SessionCount { get; set; }
}
