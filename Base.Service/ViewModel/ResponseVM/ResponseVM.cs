using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class ServiceResponseVM<T> where T : class
{
    public bool IsSuccess { get; set; }
    public string? Title { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    //public bool? IsRestored { get; set; } = false;

    public T? Result { get; set; }
}

public class ServiceResponseVM
{
    public bool IsSuccess { get; set; }
    public string? Title { get; set; }
    public IEnumerable<string>? Errors { get; set; }
}

public class AuthenticateResponseVM
{
    public string? Token { get; set; }
    public UserInformationResponseVM? Result { get; set; }
}
public class UserInformationResponseVM : Auditable
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public string? FilePath { get; set; }
    public string? DisplayName { get; set; }
    public IEnumerable<RoleResponseVM> Roles { get; set; } = new List<RoleResponseVM>();
}

public class RoleResponseVM : Auditable
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}

public abstract class Auditable
{
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }
}
