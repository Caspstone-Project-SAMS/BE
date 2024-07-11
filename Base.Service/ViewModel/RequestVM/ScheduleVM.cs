using Base.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class ScheduleVM
    {
        public DateOnly Date { get; set; }
        public int ScheduleStatus { get; set; }
        public int SlotNumber { get; set; }
        public string? ClassCode { get; set; }
        public string RoomName { get; set; } = string.Empty;
        public string CreatedBy { get; set; } = "Undefined";

    }
}
