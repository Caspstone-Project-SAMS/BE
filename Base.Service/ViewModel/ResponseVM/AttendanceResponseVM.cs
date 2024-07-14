using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class AttendanceResponseVM
{
    public int AttendanceID { get; set; }
    public int AttendanceStatus { get; set; }
    public DateTime? AttendanceTime { get; set; }
    public string? Comments { get; set; }
    public Student_AttendanceResponseVM? Student { get; set; }
    public Schedule_AttendanceResponseVM? Schedule { get; set; }
}

public class Student_AttendanceResponseVM
{
    public Guid Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? StudentCode { get; set; }
}

public class Schedule_AttendanceResponseVM
{
    public int ScheduleID { get; set; }
    public DateOnly Date { get; set; }
    public int DateOfWeek { get; set; }
    public int ScheduleStatus { get; set; }
    public Slot_AttendanceResponseVM? Slot { get; set; }
    public Class_AttendanceResponseVM? Class { get; set; }
    public Room_AttendanceResponseVM? Room { get; set; }
}

public class Slot_AttendanceResponseVM
{
    public int SlotID { get; set; }
    public int? SlotNumber { get; set; }
    public int? Status { get; set; }
    public int? Order { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? Endtime { get; set; }
}

public class Class_AttendanceResponseVM
{
    public int ClassID { get; set; }
    public string? ClassCode { get; set; }
    public int? ClassStatus { get; set; }
}

public class Room_AttendanceResponseVM
{
    public int RoomID { get; set; }
    public string? RoomName { get; set; }
    public string? RoomDescription { get; set; }
    public int? RoomStatus { get; set; }
}
