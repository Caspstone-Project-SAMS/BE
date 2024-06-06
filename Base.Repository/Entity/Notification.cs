using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;
using Base.Repository.Identity;

namespace Base.Repository.Entity;

public class Notification : AuditableEntity
{
    [Key]
    public int NotificationID { get; set; }
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    public DateTime TimeStamp { get; set; }
    public bool Read { get; set; } = false;

    public Guid UserID { get; set; }
    public User? User { get; set; }

    public int NotificationTypeID { get; set; }
    public NotificationType? NotificationType { get; set; }
}