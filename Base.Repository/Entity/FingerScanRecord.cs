using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class FingerScanRecord : AuditableEntity
{
    [Key]
    public int FingerScanReportID { get; set; }
    public DateTime RecordTime { get; set; }
    public int Status { get; set; }
    public string FingerprintTemplate { get; set; } = string.Empty;
    public int MyProperty { get; set; }

    public int ScheduleID { get; set; }
    public ScheduleTable? ScheduleTable { get; set; }
}