using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class ModuleResponseVM
{
    public int ModuleID { get; set; }
    public int? Status { get; set; }
    public int? ConnectionStatus { get; set; }
    public int? Mode { get; set; }
    public bool? AutoPrepare { get; set; } = false;
    public int? PreparedMinBeforeSlot { get; set; }
    public TimeOnly? PreparedTime { get; set; }
    public bool? AutoReset { get; set; } = false;
    public int? ResetMinAfterSlot { get; set; }
    public TimeOnly? ResetTime { get; set; }
    public Employee_ModuleResponseVM? Employee { get; set; }
}

public class Employee_ModuleResponseVM
{
    public Guid UserId { get; set; }
    public Guid EmployeeID { get; set; }
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
}