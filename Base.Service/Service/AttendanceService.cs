using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidateGet _validateGet;
        public AttendanceService(IUnitOfWork unitOfWork, IValidateGet validateGet)
        {
            _unitOfWork = unitOfWork;
            _validateGet = validateGet;
        }
        public async Task<IEnumerable<Attendance>> GetAttendances(int startPage, int endPage, int? quantity, int scheduleID)
        {
            int quantityResult = 0;
            _validateGet.ValidateGetRequest(ref startPage, ref endPage, quantity, ref quantityResult);
            if (quantityResult == 0)
            {
                throw new ArgumentException("Error when get quantity per page");
            }

            var includes = new Expression<Func<Attendance, object?>>[]
            {
                s => s.Schedule,
                s => s.Student,
                s => s.Student!.Student,
                s => s.Student!.Student!.FingerprintTemplates,
                
            };

            return await _unitOfWork.AttendanceRepository
            .Get(a => a.ScheduleID == scheduleID, includes: includes)
            .AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult)
            .ToArrayAsync();
        }

        public async Task<ServiceResponseVM<Attendance>> UpdateAttendanceStatus(int scheduleID, int attendanceStatus, DateTime? attendanceTime, Guid studentID)
        {
            var existedAttendance = await _unitOfWork.AttendanceRepository.Get(a => a.ScheduleID == scheduleID && a.StudentID.Equals(studentID)).FirstOrDefaultAsync();
            if (existedAttendance is null)
            {
                return new ServiceResponseVM<Attendance>
                {
                    IsSuccess = false,
                    Title = "Update Attendance failed",
                    Errors = new string[1] { "Attandace not found" }
                };
            }

            existedAttendance.AttendanceStatus = attendanceStatus;
            existedAttendance.AttendanceTime = attendanceTime ?? DateTime.Now;

            _unitOfWork.AttendanceRepository.Update(existedAttendance);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result)
            {
                return new ServiceResponseVM<Attendance>
                {
                    IsSuccess = true,
                    Title = "Update Status successfully",
                    Result = existedAttendance
                };
            }
            else
            {
                return new ServiceResponseVM<Attendance>
                {
                    IsSuccess = false,
                    Title = "Update Status failed"
                };
            }

        }
    }
}
