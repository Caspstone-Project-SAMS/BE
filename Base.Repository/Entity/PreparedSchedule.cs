using Base.Repository.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class PreparedSchedule : AuditableEntity
{
    public int PreparationTaskID { get; set; }
    public PreparationTask? PreparationTask { get; set; }

    public int ScheduleID { get; set; }
    public Schedule? Schedule { get; set; }

    public int TotalFingerprints { get; set; }
    public int UploadedFingerprints { get; set; }
}
