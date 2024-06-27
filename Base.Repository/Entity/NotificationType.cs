using Base.Repository.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class NotificationType : AuditableEntity
{
    [Key]
    public int NotificationTypeID { get; set; }
    [Required]
    public string TypeName { get; set; } = string.Empty;
    public string? TypeDescription { get; set; }

    public IEnumerable<Notification> Notifications { get; set; } = new List<Notification>();
}
