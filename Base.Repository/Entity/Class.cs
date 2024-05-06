using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;
using Base.Repository.Identity;

namespace Base.Repository.Entity;

public class Class : AuditableEntity 
{
    [Key]
    public int ClassID { get; set; }
    [Required]
    public string ClassCode { get; set; } = string.Empty;
    public int Status { get; set; }

    public Guid LecturerID { get; set; }
    public User? Lecturer { get; set; }

    public int CourseID { get; set; }
    public Course? Course { get; set; }

    public IEnumerable<Student> Students { get; set; } = new List<Student>();

    public IEnumerable<ScheduleTable> ScheduleTables { get; set; } = new List<ScheduleTable>();
}