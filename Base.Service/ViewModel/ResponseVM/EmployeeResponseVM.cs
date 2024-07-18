using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class EmployeeResponseVM
{
    public Guid Id { get; set; }
    public Guid? EmployeeID { get; set; }
    public string? DisplayName { get; set; }
    public string? Address { get; set; }
    public DateOnly? DOB { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Department { get; set; }
    public Role_EmployeeResponseVM? Role { get; set; }
    public IEnumerable<Class_EmployeeResponseVM> ManagedClasses { get; set; } = new List<Class_EmployeeResponseVM>();
    public IEnumerable<Module_EmployeeResponseVM> Modules { get; set; } = new List<Module_EmployeeResponseVM>();
}

public class Role_EmployeeResponseVM
{
    public int RoleId { get; set; }
    public string? Name { get; set; }
    public string? NormalizedName { get; set; }
}

public class Class_EmployeeResponseVM
{
    public int ClassID { get; set; }
    public string? ClassCode { get; set; }
    public int? ClassStatus { get; set; }
}

public class Module_EmployeeResponseVM
{
    public int ModuleID { get; set; }
    public int? Status { get; set; }
    public int? Mode { get; set; }
    public bool? AutoPrepare { get; set; }
    public int? PreparedMinBeforeSlot { get; set; }
    public TimeOnly? PreparedTime { get; set; }
    public bool? AutoReset { get; set; }
    public int? ResetMinAfterSlot { get; set; }
    public TimeOnly? ResetTime { get; set; }
}
