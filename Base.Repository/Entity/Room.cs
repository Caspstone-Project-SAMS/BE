using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Room : AuditableEntity
{
    [Key]
    public int RoomID { get; set; }
    [Required]
    public string RoomName { get; set; } = string.Empty;
    public string? RoomDescription { get; set; }
    public int RoomStatus { get; set; }

    public IEnumerable<Class> Classes { get; set; } = new List<Class>();

    public IEnumerable<Schedule> Schedules { get; set; } = new List<Schedule>();
}