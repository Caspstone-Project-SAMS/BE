using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Subject : AuditableEntity
{
    [Key]
    public int SubjectID { get; set; }
    [Required]
    public string SubjectCode { get; set; } = string.Empty;

    public IEnumerable<Course> Courses { get; set; } = new List<Course>();
    public IEnumerable<Curriculum> Curriculums { get; set; } = new List<Curriculum>();
}