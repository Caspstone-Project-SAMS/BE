using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Google.Api.Gax;
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
            .Get(a => a.ScheduleID == scheduleID && !a.IsDeleted, includes: includes)
            .AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult)
            .ToArrayAsync();
        }

        public async Task<ServiceResponseVM<Attendance>> UpdateAttendanceStatus(int scheduleID, int attendanceStatus, DateTime? attendanceTime, Guid studentID)
        {
            var existedAttendance = await _unitOfWork.AttendanceRepository
                .Get(a => a.ScheduleID == scheduleID && a.StudentID.Equals(studentID) && !a.IsDeleted)
                .Include(a => a.Schedule!.Slot)
                .FirstOrDefaultAsync();
            if (existedAttendance is null)
            {
                return new ServiceResponseVM<Attendance>
                {
                    IsSuccess = false,
                    Title = "Update Attendance failed",
                    Errors = new string[1] { "Attandace not found" }
                };
            }

            // Only update attendance status within the time frame allowed (int the end of next day, not before the schedule started)
            var currentDateTime = ServerDateTime.GetVnDateTime();

            var deadline = existedAttendance.Schedule?.Date.AddDays(1).ToDateTime(new TimeOnly(23, 59, 0));
            if (currentDateTime > deadline)
            {
                return new ServiceResponseVM<Attendance>
                {
                    IsSuccess = false,
                    Title = "Update Attendance failed",
                    Errors = new string[1] { "The modify time has ended" }
                };
            }

            existedAttendance.AttendanceStatus = attendanceStatus;
            existedAttendance.AttendanceTime = attendanceTime ?? ServerDateTime.GetVnDateTime();

            // Chấm công tại đây
            var existedSchedule = _unitOfWork.ScheduleRepository
                .Get(s => s.ScheduleID == scheduleID)
                .FirstOrDefault();
            if(existedSchedule is not null)
            {
                if(existedSchedule.Attended == 1)
                {
                    existedSchedule.Attended = 2;
                }
            }

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

            // Only update attendance status within the time frame allowed (int the end of next day, not before the schedule started)
            var currentDateTime = ServerDateTime.GetVnDateTime();


            foreach (var student in studentArr)
            {
                try
                {
                    var existedAttendance = await _unitOfWork.AttendanceRepository
                        .Get(a => a.ScheduleID == student.ScheduleID && a.StudentID == student.StudentID)
                        .Include(a => a.Schedule!.Slot)
                        .FirstOrDefaultAsync();

                    if (existedAttendance == null)
                    {
                        errors.Add($"Attendance not found for student ID {student.StudentID} in schedule ID {student.ScheduleID}");
                        continue;
                    }

                    var deadline = existedAttendance.Schedule?.Date.AddDays(1).ToDateTime(new TimeOnly(23, 59, 0));
                    if (currentDateTime > deadline)
                    {
                        errors.Add("The modify time has ended.");
                        continue;
                    }

                    existedAttendance.AttendanceStatus = student.AttendanceStatus;
                    existedAttendance.AttendanceTime = student.AttendanceTime ?? ServerDateTime.GetVnDateTime();
                    existedAttendance.Comments = student.Comments;

                    _unitOfWork.AttendanceRepository.Update(existedAttendance);
                    responseList.Add(student);
                }
                catch (Exception ex)
                {
                    errors.Add($"Error updating attendance for student ID {student.StudentID} in schedule ID {student.ScheduleID}: {ex.Message}");
                    continue;
                }
            }

            var scheduleIds = studentArr.Select(a => a.ScheduleID).Distinct().ToArray();
            if (scheduleIds.Count() > 0)
            {
                foreach (int scheduleId in scheduleIds)
                {
                    var existedSchedule = _unitOfWork.ScheduleRepository
                        .Get(s => s.ScheduleID == scheduleId)
                        .FirstOrDefault();

                    if (existedSchedule is not null)
                    {
                        if (existedSchedule.Attended == 1)
                        {
                            existedSchedule.Attended = 2;
                        }
                    }
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

        public async Task<IEnumerable<AttendanceReportResponse>> GetAttendanceReport(int classId)
        {
            DbContextFactory dbFactory = new DbContextFactory();

            var dbContext1 = dbFactory.CreateDbContext(Array.Empty<string>());
            var dbContext2 = dbFactory.CreateDbContext(Array.Empty<string>());

            var studentsTask = dbContext1
                .Set<User>()
                .Where(u => u.StudentClasses.Any(sc => sc.ClassID == classId))
                .Include(u => u.StudentClasses.Where(sc => sc.ClassID == classId))
                .Include(u => u.Student)
                .AsNoTracking()
                .ToListAsync();

            var scheduleTask = dbContext2
                .Set<Schedule>()
                .Where(s => s.ClassID == classId)
                .Include(s => s.Attendances)
                .Include(s => s.Slot)
                .Include(s => s.Class)
                .AsNoTracking()
                .ToListAsync();

            await Task.WhenAll(studentsTask, scheduleTask);

            var students = await studentsTask;
            var schedules = await scheduleTask;

            var result = students.Select(student => new AttendanceReportResponse
            {
              
                StudentCode = student!.Student!.StudentCode,
                StudentName = student.DisplayName,
                AbsencePercentage = student.StudentClasses.First(sc => sc.ClassID == classId).AbsencePercentage,
                AttendanceRecords = schedules.Select(schedule =>
                {
                    var attendance = schedule.Attendances.FirstOrDefault(a => a.StudentID == student.Id);
                    var slotNumber = schedule.Slot?.SlotNumber;
                    return new AttendanceRecord
                    {
                        Date = schedule.Date,
                        SlotNumber = slotNumber,
                        Status = attendance != null ? attendance.AttendanceStatus : -1
                    };
                }).OrderBy(record => record.Date)
                .ThenBy(record => record.SlotNumber)
                .ToList()
            }).OrderBy(s => s.AttendanceRecords!.Min(a => a.Date)).ToList();

            dbContext1.Dispose();
            dbContext2.Dispose();

            return result;
        }
    }
}
