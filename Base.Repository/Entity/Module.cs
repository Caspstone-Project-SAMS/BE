using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Module : AuditableEntity
{
    [Key]
    public int ModuleID { get; set; }
    public int Status { get; set; }
    public int Mode { get; set; }
    public string Token { get; set; } = string.Empty;

    public int? RoomID { get; set; }
    public Room? Room { get; set; }
}