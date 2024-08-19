using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class SlotVM
    {
        public int? SlotNumber { get; set; }
        public int? Status { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? Endtime { get; set; }
    }
}
