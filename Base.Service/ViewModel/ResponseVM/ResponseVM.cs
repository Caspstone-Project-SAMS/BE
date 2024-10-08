﻿using Base.Repository.Identity;
using Base.Service.CustomJsonConverter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json;
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
    public Guid? StudentID { get; set; }
    public Guid? EmployeeID { get; set; }
    public string? Email { get; set; }
    public string? NormalizedEmail { get; set; }
    public bool EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public bool LockoutEnabled { get; set; }
    public string? Avatar { get; set; }
    public string? DisplayName { get; set; }
    public string? Address { get; set; }
    public DateOnly? DOB { get; set; }
    public int? Gender { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
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
    public int ClassID { get; set; }
    [JsonConverter(typeof(DateOnlyJsonConverter))]
    public DateOnly Date { get; set; }
    public int SlotNumber { get; set; }
    public int ScheduleStatus { get; set; }
    public string? ClassCode { get; set; }
    public string? SubjectCode { get; set; }
    public string? RoomName { get; set; }
    public string? AttendStudent { get; set; }
    public int Attended { get; set; }
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
}


public class StudentResponse
{
    public string? StudentName { get; set; }
    public Guid? UserID { get; set; }
    public Guid? StudentID { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? StudentCode { get; set; }
    public int AbsencePercentage { get; set; }
    public bool IsAuthenticated { get; set; } = false;
}
public class StudentModuleResponse
{
    public string? StudentName { get; set; }
    public Guid? UserID { get; set; }
    public Guid? StudentID { get; set; }
    public string? StudentCode { get; set; }
    public IEnumerable<string>? FingerprintTemplateData { get; set; }
}

public class AttendanceResponse 
{
    public int AttendanceStatus { get; set; }
    public string? Comments { get; set; }
    public Guid? StudentID { get; set; }
    public string? StudentCode { get; set; }
    public string? StudentName { get; set; }
    public string? Email { get; set; }
    public bool IsAuthenticated { get; set; } = false;
    public string? Avatar {  get; set; } = string.Empty;
}

public class RoomResponse
{
    public int RoomID { get; set; }
    public string RoomName { get; set; } = string.Empty;
    public string? RoomDescription { get; set; }
    public int RoomStatus { get; set; }
}

public class SubjectResponse
{
    public int SubjectID { get; set; }
    public string SubjectCode { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
}

public class SlotResponse
{
    public int SlotID { get; set; }
    public int SlotNumber { get; set; }
    public int Status { get; set; }
    public int Order { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly Endtime { get; set; }
}
public abstract class Auditable
{
    public string CreatedBy { get; set; } = "Undefined";
    public DateTime CreatedAt { get; set; }
}


public class ImportServiceResposneVM<T> where T : class
{
    public bool IsSuccess { get; set; }
    public string? Title { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    //public bool? IsRestored { get; set; } = false;

    public IEnumerable<T> ImportedEntities { get; set; } = Enumerable.Empty<T>();
    public IEnumerable<ImportErrorEntity<T>> ErrorEntities { get; set; } = Enumerable.Empty<ImportErrorEntity<T>>();
}

public class ImportErrorEntity<T> where T : class
{
    public T? ErrorEntity { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}
