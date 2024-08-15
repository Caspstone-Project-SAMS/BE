using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM;

public class ModuleVM
{
    // Attendance
    public bool? AutoPrepare { get; set; } = false;
    public string? PreparedTime { get; set; }
    public int? AttendanceDurationMinutes { get; set; }

    public int? ConnectionLifeTimeSeconds { get; set; }

    public bool? ConnectionSound { get; set; }
    public int? ConnectionSoundDurationMs { get; set; }
    public bool? AttendanceSound { get; set; }
    public int? AttendanceSoundDurationMs { get; set; }
}
