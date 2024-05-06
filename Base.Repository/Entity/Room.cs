using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Room : AuditableEntity
{
    [Key]
    public int RoomID { get; set; }
    [Required]
    public string RoomName { get; set; } = string.Empty;

    public int CampusID { get; set; }
    public Campus? Campus { get; set; }

    public Module? Module { get; set; }
}