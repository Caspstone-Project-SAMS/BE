using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Module : AuditableEntity
{
    [Key]
    public int ModuleID { get; set; }
    public int Status { get; set; }
    public int Mode { get; set; }
    public string Key { get; set; } = string.Empty;

    // Config-Setup
    public bool AutoPrepare { get; set; } = false;
    public int? PreparedMinBeforeSlot { get; set; }
    public TimeOnly? PreparedTime { get; set; }
    public bool AutoReset { get; set; } = false;
    public int? ResetMinAfterSlot { get; set; }
    public TimeOnly? ResetTime { get; set; }

    public Guid EmployeeID { get; set; }
    public Employee? Employee { get; set; }
}