using Base.Repository.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public RoleResponseVM Role { get; set; } = new RoleResponseVM();
}

public class RoleResponseVM : Auditable
{
    public int RoleId { get; set; }
    public string? Name { get; set; }
}

public class ScheduleResponse
{
    public int ScheduleID { get; set; }
    public DateOnly Date { get; set; }
    public int SlotNumber { get; set; }
    public string? ClassCode { get; set; }
    public string? SubjectCode { get; set; }
    public string? RoomName { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }

}

public class SemesterResponse
{
    public int SemesterID { get; set; }
    public string SemesterCode { get; set; } = string.Empty;
    public int SemesterStatus { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public class ClassResponse
{
    public int ClassID { get; set; }
    public DateOnly Date { get; set; }
    
    public string? ClassCode { get; set; }

    public string? RoomName { get; set; }
    public string? LecturerName { get; set; }
    public string? SubjectCode { get; set; }
    public string? SubjectName { get; set; }
    public IEnumerable<User> Students { get; set; } = new List<User>();
}

public class StudentResponse
{
    public string? StudentName { get; set; }
    public string? Image { get; set; }
    public string? StudentCode { get; set; }
    public string FingerprintTemplateData { get; set; } = string.Empty;
}
public abstract class Auditable
{
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }
}
