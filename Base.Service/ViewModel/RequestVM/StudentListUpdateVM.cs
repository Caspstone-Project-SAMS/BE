using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class StudentListUpdateVM
    {
        public int ScheduleID { get; set; }
        public int AttendanceStatus { get; set; }
        public DateTime? AttendanceTime { get; set; }
        public Guid? StudentID { get; set; }
        public string Comments { get; set; } = string.Empty;
    }
}
