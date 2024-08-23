using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Base.Repository.Common;
using Base.Repository.Identity;

namespace Base.Repository.Entity;

public class Class : AuditableEntity, ICloneable
{
    [Key]
    public int ClassID { get; set; }
    [Required]
    public string ClassCode { get; set; } = string.Empty;
    public int ClassStatus { get; set; }

    public int SemesterID { get; set; }
    public Semester? Semester { get; set; }

    public int RoomID { get; set; }
    public Room? Room { get; set; }

    public int SubjectID { get; set; }
    public Subject? Subject { get; set; }


    // A class is managed by a lecturer
    public Guid LecturerID { get; set; }
    public User? Lecturer { get; set; }


    // A class have many students
    public IEnumerable<User> Students { get; set; } = new List<User>();
    public IEnumerable<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();


    public IEnumerable<Schedule> Schedules { get; set; } = new List<Schedule>();

    public int GetAbsencePercentage()
    {
        return this.StudentClasses.FirstOrDefault()?.AbsencePercentage ?? 0;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}