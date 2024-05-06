using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Course : AuditableEntity
{
    [Key]
    public int CourseID { get; set; }

    public int Status { get; set; }

    public int SemesterID { get; set; }
    public Semester? Semester { get; set; }

    public int SubjectID { get; set; }
    public Subject? Subject { get; set; }

    public IEnumerable<Class> Classes { get; set; } = new List<Class>();
}