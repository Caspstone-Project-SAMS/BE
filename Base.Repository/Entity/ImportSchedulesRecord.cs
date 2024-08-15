using Base.Repository.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class ImportSchedulesRecord
{
    public int ImportSchedulesRecordID { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime RecordTimestamp { get; set; }
    public bool ImportReverted { get; set; } = false;
    public bool IsReversible { get; set; } = false;
    public IEnumerable<Schedule> ImportedSchedules { get; set; } = new List<Schedule>();

    public Guid UserId { get; set; }
    public User? User { get; set; }
}
