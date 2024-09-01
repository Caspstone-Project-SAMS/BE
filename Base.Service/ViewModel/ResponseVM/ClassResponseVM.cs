using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class ClassResponseVM
{
    public int ClassID { get; set; }
    public string? ClassCode { get; set; } = string.Empty;
    public int? ClassStatus { get; set; }
    public SlotType_ClassResponseVM? SlotType { get; set; }
    public Semester_ClassResponseVM? Semester { get; set; }
    public Room_ClassResponseVM? Room { get; set; }
    public Subject_ClassResponseVM? Subject { get; set; }
    public Lecturer_ClassResponseVM? Lecturer { get; set; }
    public IEnumerable<Student_ClassResponseVM> Students { get; set; } = new List<Student_ClassResponseVM>();
    public IEnumerable<Schedule_ClassResponseVM> Schedules { get; set; } = new List<Schedule_ClassResponseVM>();
}

public class Semester_ClassResponseVM
{
    public int SemesterID { get; set; }
    public string? SemesterCode { get; set; }
    public int? SemesterStatus { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class Room_ClassResponseVM
{
    public int RoomID { get; set; }
    public string? RoomName { get; set; }
    public string? RoomDescription { get; set; }
    public int? RoomStatus { get; set; }
}

public class Subject_ClassResponseVM
{
    public int SubjectID { get; set; }
    public string? SubjectCode { get; set; }
    public string? SubjectName { get; set; }
    public int? SubjectStatus { get; set; }
}

public class Lecturer_ClassResponseVM
{
    public Guid Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? Department { get; set; }
}

public class Student_ClassResponseVM
{
    public Guid Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? StudentCode { get; set; }
    public int? AbsencePercentage { get; set; }
}

public class Schedule_ClassResponseVM
{
    public int ScheduleID { get; set; }
    public DateOnly? Date { get; set; }
    public int? DateOfWeek { get; set; }
    public int? ScheduleStatus { get; set; }
    public int? Attended { get; set; }
    public Slot_ClassResponseVM? Slot { get; set; }
    public Room_Schedule_ClassResponseVM? Room { get; set; }
}

public class Room_Schedule_ClassResponseVM
{
    public int RoomID { get; set; }
    public string? RoomName { get; set; }
    public string? RoomDescription { get; set; }
    public int? RoomStatus { get; set; }
}

public class Slot_ClassResponseVM
{
    public int SlotID { get; set; }
    public int? SlotNumber { get; set; }
    public int? Status { get; set; }
    public int? Order { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? Endtime { get; set; }
}

public class SlotType_ClassResponseVM
{
    public int SlotTypeID { get; set; }
    public string? TypeName { get; set; }
    public string? Description { get; set; }
    public int? Status { get; set; }
    public int? SessionCount { get; set; }
}