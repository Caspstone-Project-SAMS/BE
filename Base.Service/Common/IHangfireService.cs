using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Common;

public interface IHangfireService
{
    Task SetSlotStart(int slotId, DateOnly? date);
    Task SetSlotEnd(int slotId, DateOnly? date);
    void SetRecordIrreversible(int recordId, DateTime endTimeStamp);
    void SendEmailReCheckAttendance(int slotId, DateOnly date);
}
