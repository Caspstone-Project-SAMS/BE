using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.ResponseVM;

public class ImportScheduleServiceResponseVM
{
    public bool IsSuccess { get; set; }
    public string? Title { get; set; }
    public IEnumerable<string>? Errors { get; set; }
    public IEnumerable<Schedule_ImportScheduleServiceResponseVM> ImportedEntities { get; set; } = Enumerable.Empty<Schedule_ImportScheduleServiceResponseVM>();
    public IEnumerable<ImportErrorEntity<Schedule_ImportScheduleServiceResponseVM>> ErrorEntities { get; set; } = Enumerable.Empty<ImportErrorEntity<Schedule_ImportScheduleServiceResponseVM>>();
}

public class Schedule_ImportScheduleServiceResponseVM
{
    public int ScheduleID { get; set; }
    public DateOnly? Date { get; set; }
    public int? DateOfWeek { get; set; }
    public int? ScheduleStatus { get; set; }
    public int? SlotNumber { get; set; }
    public string? ClassCode { get; set; }
}
