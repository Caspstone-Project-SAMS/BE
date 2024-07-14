using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class SemesterResponseVM
{
    public int SemesterID { get; set; }
    public string? SemesterCode { get; set; }
    public int? SemesterStatus { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public IEnumerable<Class_SemesterResponseVM> Classes { get; set; } = new List<Class_SemesterResponseVM>();
}

public class Class_SemesterResponseVM
{
    public int ClassID { get; set; }
    public string? ClassCode { get; set; }
    public int? ClassStatus { get; set; }
}
