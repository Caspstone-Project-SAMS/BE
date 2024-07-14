using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class StudentResponseVM
{
    public Guid Id { get; set; }
    public string? DisplayName { get; set; }
    public string? Address { get; set; }
    public DateOnly? DOB { get; set; }
    public string? Avatar { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? StudentCode { get; set; }
    public IEnumerable<FingerprintTemplate_StudentResponseVM> FingerprintTemplates { get; set; } = new List<FingerprintTemplate_StudentResponseVM>();
    public IEnumerable<Class_StudentResponseVM> EnrolledClasses { get; set; } = new List<Class_StudentResponseVM>();
}

public class FingerprintTemplate_StudentResponseVM
{
    public int FingerprintTemplateID { get; set; }
    public int? Status { get; set; }
}

public class Class_StudentResponseVM
{
    public int ClassID { get; set; }
    public string? ClassCode { get; set; }
    public int? ClassStatus { get; set; }
    public int? AbsencePercentage { get; set; }
}
