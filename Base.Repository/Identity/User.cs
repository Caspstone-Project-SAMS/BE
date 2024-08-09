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
    public string? Address { get; set; }
    public DateOnly? DOB { get; set; }
    public string? Avatar { get; set; }
    public bool Deleted { get; set; } = false;
    public bool IsActivated { get; set; } = false;
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }


    // =======================Role=====================//
    public int? RoleID { get; set; }
    public Role? Role { get; set; }
    

    // ==================================== For student======================= //
    // This account could be student (nullable)
    public Guid? StudentID { get; set; }
    public Student? Student { get; set; }

    // A list of classes that the student participate in
    public IEnumerable<Class> EnrolledClasses { get; set; } = new List<Class>();
    public IEnumerable<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();


    // ==================================== For teacher======================= //
    // This account could be employee
    public Guid? EmployeeID { get; set; }
    public Employee? Employee { get; set; }

    // A list of classes that the teacher teaches
    public IEnumerable<Class> ManagedClasses { get; set; } = new List<Class>();


    // ============================Notification============================//
    public IEnumerable<Notification> Notifications { get; set; } = new List<Notification>();


    // ============================Attendance======================//
    public IEnumerable<Attendance> Attendances { get; set; } = new List<Attendance>();


    // ============================Substitute teaching===================//
    public IEnumerable<SubstituteTeaching> SubstituteTeachings { get; set; } = new List<SubstituteTeaching>();


    // ===========================Officialy assigned teaching===========================//
    public IEnumerable<SubstituteTeaching> AssignedTeachings { get; set; } = new List<SubstituteTeaching>();


    // ===========================Import Record========================//
    public IEnumerable<ImportSchedulesRecord> ImportSchedulesRecords { get; set; } = new List<ImportSchedulesRecord>();

    public Role? GetRole()
    {
        if (this.Role is not null && this.Role.Deleted != true)
        {
            return this.Role;
        }
        return null;
    }

    public int GetAbsencePercentage()
    {
        return this.StudentClasses.FirstOrDefault()?.AbsencePercentage ?? 0;
    }
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
