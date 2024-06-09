using Base.Repository.Entity;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService
{
    public interface IAttendanceService
    {
        Task<IEnumerable<Attendance>> GetAttendances(int startPage, int endPage, int? quantity,int scheduleID);
        Task<ServiceResponseVM<Attendance>> UpdateAttendanceStatus(int attendanceID,int attendanceStatus,DateTime? attendanceTime,Guid studentID);
    }
}
