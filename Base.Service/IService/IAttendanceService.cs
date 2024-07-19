using Base.Repository.Entity;
using Base.Service.ViewModel.RequestVM;
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
        Task<ServiceResponseVM<Attendance>> UpdateAttendanceStatus( int scheduleID, int attendanceStatus,DateTime? attendanceTime,Guid studentID);

        Task<ServiceResponseVM<List<StudentListUpdateVM>>> UpdateListStudentStatus(StudentListUpdateVM[] studentArr);

        Task<ServiceResponseVM<IEnumerable<Attendance>>> GetAttendanceList(int startPage, int endPage, int quantity, int? attendanceStatus, int? scheduleID, Guid? studentId, int? classId);

        Task<Attendance?> GetAttendanceById(int attendanceID);
    }
}
