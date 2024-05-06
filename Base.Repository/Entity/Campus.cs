using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Campus : AuditableEntity
{
    [Key]
    public int CampusID { get; set; }
    [Required]
    public string CampusName { get; set; } = string.Empty;
    
    public IEnumerable<Room> Rooms { get; set; } = new List<Room>();
}