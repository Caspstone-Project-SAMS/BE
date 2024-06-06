using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Subject : AuditableEntity
{
    [Key]
    public int SubjectID { get; set; }
    [Required]
    public string SubjectCode { get; set; } = string.Empty;
    [Required]
    public string SubjectName { get; set; } = string.Empty;
    public int SubjectStatus { get; set; }

    public IEnumerable<Class> Classes { get; set; } = new List<Class>();
}