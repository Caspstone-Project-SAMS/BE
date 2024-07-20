using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class ModuleVM
    {
        public bool AutoPrepare { get; set; } = false;
        //public int? PreparedMinBeforeSlot { get; set; }
        public string? PreparedTime { get; set; }
        //public bool AutoReset { get; set; } = false;
        //public int? ResetMinAfterSlot { get; set; }
        //public TimeOnly? ResetTime { get; set; }
    }
}
