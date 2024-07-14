using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class ScheduleResponseVM
{
    public int ScheduleID { get; set; }
    public DateOnly? Date { get; set; }
    public int? DateOfWeek { get; set; }
    public int? ScheduleStatus { get; set; }
    public Slot_ScheduleResponseVM? Slot { get; set; }
    public Class_ScheduleResponseVM? Class { get; set; }
    public Room_ScheduleResponseVM? Room { get; set; }
    public IEnumerable<Attendance_ScheduleResponseVM> Attendances { get; set; } = new List<Attendance_ScheduleResponseVM>();
}

public class Slot_ScheduleResponseVM
{
    public int SlotID { get; set; }
    public int? SlotNumber { get; set; }
    public int? Status { get; set; }
    public int? Order { get; set; }
    public TimeOnly? StartTime { get; set; }
    public TimeOnly? Endtime { get; set; }
}

public class Class_ScheduleResponseVM
{
    public int ClassID { get; set; }
    public string? ClassCode { get; set; }
    public int? ClassStatus { get; set; }
}

public class Room_ScheduleResponseVM
{
    public int RoomID { get; set; }
    public string? RoomName { get; set; }
    public string? RoomDescription { get; set; }
    public int? RoomStatus { get; set; }
}

public class Attendance_ScheduleResponseVM
{
    public int AttendanceID { get; set; }
    public int? AttendanceStatus { get; set; }
    public DateTime? AttendanceTime { get; set; }
    public string? Comments { get; set; }
    public Student_ScheduleResponseVM? Student { get; set; }
}

public class Student_ScheduleResponseVM
{
    public Guid Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? StudentCode { get; set; }
}
