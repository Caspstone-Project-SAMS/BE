using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Semester : AuditableEntity
{
    [Key]
    public int SemesterID { get; set; }
    public string SemesterCode { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }

    public IEnumerable<Course> Courses { get; set; } = new List<Course>();
}