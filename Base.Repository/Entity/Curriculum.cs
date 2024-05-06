using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Curriculum : AuditableEntity
{
    [Key]
    public int CurriculumID { get; set; }

    public IEnumerable<Subject> Subjects { get; set; } = new List<Subject>();
    public IEnumerable<Student> Students { get; set; } = new List<Student>();
}