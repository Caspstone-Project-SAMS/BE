using System.ComponentModel.DataAnnotations;
using Base.Repository.Common;

namespace Base.Repository.Entity;

public class ScheduleTable : AuditableEntity
{
    [Key]
    public int ScheduleTableID { get; set; }
    public DateOnly Date { get; set; }
    public int DateOfWeek { get; set; }

    public int RoomID { get; set; }
    public Room? Room { get; set; }

    public int SlotID { get; set; }
    public Slot? Slot { get; set; }

    public int ClassID { get; set; }
    public Class? Class { get; set; }

    public IEnumerable<AttendanceReport> AttendanceReports { get; set; } = new List<AttendanceReport>();

    public IEnumerable<FingerScanRecord> FingerScanRecords { get; set; } = new List<FingerScanRecord>();
}