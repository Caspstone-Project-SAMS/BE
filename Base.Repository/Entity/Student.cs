using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class Student : AuditableEntity
{
    [Key]
    public Guid StudentID { get; set; }
    [Required]
    public string StudentCode { get; set; } = string.Empty;
    [Required]
    public string Email { get; set; } = string.Empty;
    public string NormalizedEmail { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;
    [Required]
    public string Phone { get; set; } = string.Empty;

    public int CurriculumID { get; set; }
    public Curriculum? Curriculum { get; set; }

    public IEnumerable<Class> Classes { get; set; } = new List<Class>();

    public IEnumerable<FingerprintTemplate> FingerprintTemplates { get; set; } = new List<FingerprintTemplate>();

    public IEnumerable<AttendanceReport> AttendanceReports { get; set; } = new List<AttendanceReport>();
}