using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;
using Base.Repository.Identity;

namespace Base.Repository.Entity;

public class Student : AuditableEntity
{
    [Key]
    public Guid StudentID { get; set; }
    [Required]
    public string StudentCode { get; set; } = string.Empty;

    public User? User { get; set; }

    public IEnumerable<FingerprintTemplate> FingerprintTemplates { get; set; } = new List<FingerprintTemplate>();
}