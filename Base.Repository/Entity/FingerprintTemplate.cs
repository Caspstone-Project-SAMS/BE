using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class FingerprintTemplate : AuditableEntity
{
    [Key]
    public int FingerprintTemplateID { get; set; }
    public int Status { get; set; }
    public string FingerprintTemplateData { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid StudentID { get; set; }
    public Student? Student { get; set; }
}