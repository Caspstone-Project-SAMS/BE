using Base.Repository.Common;
using Base.Repository.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class Employee : AuditableEntity
{
    [Key]
    public Guid EmployeeID { get; set; }
    public string Department { get; set; } = string.Empty;

    public User? User { get; set; }

    public IEnumerable<Module> Modules { get; set; } = new List<Module>();
}
