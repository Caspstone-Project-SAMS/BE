using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class NotificationResponseVM
{
    public int NotificationID { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? TimeStamp { get; set; }
    public bool? Read { get; set; }
    public int? ModuleId { get; set; }
    public int? ModuleActivityId { get; set; }
    public int? ScheduleID { get; set; }
    public NotificationType_NotificationResponseVM? NotificationType { get; set; }
    public User_NotificationResponseVM? User { get; set; }
}

public class NotificationType_NotificationResponseVM
{
    public int NotificationTypeID { get; set; }
    public string? TypeName { get; set; }
    public string? TypeDescription { get; set; }
}

public class User_NotificationResponseVM
{
    public Guid Id { get; set; }
    public Guid? EmployeeID { get; set; }
    public Guid? StudentID { get; set; }
    public string? DisplayName { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
}
