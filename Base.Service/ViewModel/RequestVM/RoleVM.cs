using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM.Role;

public class RoleVM
{
    [Required]
    public string? RoleName { get; set; }
}

public class UpdateRoleVM
{
    public string? RoleName { get; set; }
}
