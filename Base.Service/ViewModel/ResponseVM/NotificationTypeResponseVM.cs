using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class NotificationTypeResponseVM
{
    public int NotificationTypeID { get; set; }
    public string? TypeName { get; set; }
    public string? TypeDescription { get; set; }
    public IEnumerable<Notification_NotificationTypeResponseVM> Notifications { get; set; } = new List<Notification_NotificationTypeResponseVM>();
}

public class Notification_NotificationTypeResponseVM
{
    public int NotificationID { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTime? TimeStamp { get; set; }
    public bool? Read { get; set; }
}
