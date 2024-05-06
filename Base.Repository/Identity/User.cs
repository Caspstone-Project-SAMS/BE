using Base.Repository.Common;
using Base.Repository.Entity;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Identity;

public class User : IdentityUser<Guid>
{
    public string? DisplayName { get; set; } = "Undefined";
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }

    public string? FilePath { get; set; }

    public bool Deleted { get; set; } = false;

    public bool IsActivated { get; set; } = false;

    public IEnumerable<Role> Roles { get; set; } = new List<Role>();

    public IEnumerable<Class> Classes { get; set; } = new List<Class>();
}

public class LoginUserManagement
{
    public string? Title { get; set; }
    public bool IsSuccess { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public User? LoginUser { get; set; }
    public string? ConfirmEmailUrl { get; set; }
    public IEnumerable<string>? RoleNames { get; set; }
}
