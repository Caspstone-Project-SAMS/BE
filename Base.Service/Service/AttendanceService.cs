using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
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
            existedAttendance.AttendanceTime = attendanceTime ?? ServerDateTime.GetVnDateTime();

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

        public async Task<ServiceResponseVM<List<StudentListUpdateVM>>> UpdateListStudentStatus(StudentListUpdateVM[] studentArr)
        {
           
                var responseList = new List<StudentListUpdateVM>();
                var errors = new List<string>();

                foreach (var student in studentArr)
                {
                    try
                    {
                        var existedAttendance = await _unitOfWork.AttendanceRepository
                            .Get(a => a.ScheduleID == student.ScheduleID && a.StudentID == student.StudentID)
                            .FirstOrDefaultAsync();

                        if (existedAttendance == null)
                        {
                            errors.Add($"Attendance not found for student ID {student.StudentID} in schedule ID {student.ScheduleID}");
                            continue;
                        }

                        existedAttendance.AttendanceStatus = student.AttendanceStatus;
                        existedAttendance.AttendanceTime = student.AttendanceTime ?? ServerDateTime.GetVnDateTime();
                        existedAttendance.Comments = student.Comments;

                    _unitOfWork.AttendanceRepository.Update(existedAttendance);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Error updating attendance for student ID {student.StudentID} in schedule ID {student.ScheduleID}: {ex.Message}");
                        continue;
                    }
                }

                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponseVM<List<StudentListUpdateVM>>
                    {
                        IsSuccess = true,
                        Title = "Update Status successfully",
                        Result = responseList
                    };
                }
                else
                {
                    return new ServiceResponseVM<List<StudentListUpdateVM>>
                    {
                        IsSuccess = false,
                        Title = "Update Status failed",
                        Errors = errors.ToArray()
                    };
                }
            

        }

        public async Task<Attendance?> GetAttendanceById(int attendanceID)
        {
            var includes = new Expression<Func<Attendance, object?>>[]
            {
                a => a.Schedule,
                a => a.Schedule!.Class,
                a => a.Schedule!.Slot,
                a => a.Student!.Student
            };
            return await _unitOfWork.AttendanceRepository
                .Get(a => a.AttendanceID == attendanceID, includes)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<ServiceResponseVM<IEnumerable<Attendance>>> GetAttendanceList(int startPage, int endPage, int quantity, int? attendanceStatus, int? scheduleID, Guid? studentId, int? classId)
        {
            var result = new ServiceResponseVM<IEnumerable<Attendance>>()
            {
                IsSuccess = false
            };
            var errors = new List<string>();

            int quantityResult = 0;
            _validateGet.ValidateGetRequest(ref startPage, ref endPage, quantity, ref quantityResult);
            if (quantityResult == 0)
            {
                errors.Add("Invalid get quantity");
                result.Errors = errors;
                return result;
            }

            var includes = new Expression<Func<Attendance, object?>>[]
            {
                s => s.Schedule,
                s => s.Student,
                s => s.Student!.Student,
                s => s.Schedule!.Class,
                s => s.Schedule!.Class!.Room,
                s => s.Schedule!.Slot,
                s => s.Schedule!.Room,
            };

            var queryAttendance = _unitOfWork.AttendanceRepository.Get(a => !a.IsDeleted, includes);

            if(attendanceStatus is not null)
            {
                queryAttendance = queryAttendance.Where(a => a.AttendanceStatus == attendanceStatus);
            }

            if(scheduleID is not null)
            {
                queryAttendance = queryAttendance.Where(a => a.ScheduleID == scheduleID);
            }

            if(studentId is not null)
            {
                queryAttendance = queryAttendance.Where(a => a.StudentID == studentId);
            }

            if(classId is not null)
            {
                queryAttendance = queryAttendance.Where(a => a.Schedule!.Class!.ClassID == classId);
            }

            result.IsSuccess = true;
            result.Result = await queryAttendance
                .AsNoTracking()
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult)
                .ToArrayAsync();
            result.Title = "Get successfully";

            return result;
        }
    }
}
