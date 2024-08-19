using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class ModuleActivityResponseVM
{
    public int ModuleActivityId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public bool? IsSuccess { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    public PreparationTask_ModuleActivityResponseVM? PreparationTask { get; set; }

    public Module_ModuleActivityResponseVM? Module { get; set; }
}

public class PreparationTask_ModuleActivityResponseVM
{
    public float? Progress { get; set; }
    public int? PreparedScheduleId { get; set; }
    public int TotalFingers { get; set; }
    public int UploadedFingers { get; set; }
    public IEnumerable<int> PreparedSchedules { get; set; } = new List<int>();
}

public class Module_ModuleActivityResponseVM
{
    public int ModuleID { get; set; }
    public int? Status { get; set; }
    public int? ConnectionStatus { get; set; }
    public int? Mode { get; set; }
    public bool? AutoPrepare { get; set; } = false;
}
