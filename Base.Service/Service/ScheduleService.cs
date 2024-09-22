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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class ScheduleService : IScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidateGet _validateGet;
        private readonly ICurrentUserService _currentUserService;
        private readonly IHangfireService _hangfireService;

        public ScheduleService(IUnitOfWork unitOfWork, IValidateGet validateGet, ICurrentUserService currentUserService, IHangfireService hangfireService)
        {
            _unitOfWork = unitOfWork;
            _validateGet = validateGet;
            _currentUserService = currentUserService;
            _hangfireService = hangfireService;
        }

        public async Task<ServiceResponseVM<List<ScheduleVM>>> Create(List<ScheduleVM> newEntities, int semesterId)
        {
            List<ScheduleVM> createdSchedule = new List<ScheduleVM>();
            List<string> errors = new List<string>();
            var existedSemester = await _unitOfWork.SemesterRepository.Get(s => s.SemesterID == semesterId && !s.IsDeleted).SingleOrDefaultAsync();
            if (existedSemester is null)
            {
                return new ServiceResponseVM<List<ScheduleVM>>
                {
                    IsSuccess = false,
                    Title = "Add new Schedule failed",
                    Errors = new string[1] { "Semester not Existed" }
                };
            }
            foreach (var newEntity in newEntities)
            {
                
                    var existedClass = await _unitOfWork.ClassRepository.Get(c => c.ClassCode.Equals(newEntity.ClassCode)&& c.SemesterID == semesterId && !c.IsDeleted).Include(s => s.Semester).FirstOrDefaultAsync();
                    if (existedClass is null)
                    {
                        errors.Add($"Class with Class Code: {newEntity.ClassCode} not existed in Semester {existedSemester.SemesterCode}");
                        continue;
                    }

                    var existedSlot = await _unitOfWork.SlotRepository.Get(s => s.SlotNumber == newEntity.SlotNumber && s.SlotTypeId == existedClass.SlotTypeId && !s.IsDeleted).FirstOrDefaultAsync();
                    if( existedSlot is null)
                    {
                        errors.Add($"Slot {newEntity.SlotNumber} not existed");
                        continue;
                    }

                TimeOnly newSlotStartTime = existedSlot.StartTime;
                TimeOnly newSlotEndTime = existedSlot.Endtime;
                var overlappingSchedules = await _unitOfWork.ScheduleRepository.Get(
                                s => s.Date == newEntity.Date
                                     && s.SlotID != existedSlot.SlotID
                                     && s.Class!.RoomID != existedClass.RoomID
                                     && !s.IsDeleted)
                                .Include(s => s.Slot) 
                                .ToArrayAsync();

                foreach (var schedule in overlappingSchedules)
                {
                    TimeOnly existingSlotStartTime = schedule!.Slot!.StartTime;
                    TimeOnly existingSlotEndTime = schedule!.Slot.Endtime;
                    if (newSlotStartTime < existingSlotEndTime && newSlotEndTime > existingSlotStartTime)
                    {
                        errors.Add($"Slot {newEntity.SlotNumber} overlaps with another class in room {existedClass.ClassCode} on {newEntity.Date}");
                        continue;
                    }
                }
                var existedSchedule = await _unitOfWork.ScheduleRepository.Get(
                                                                                s => s.Date == newEntity.Date 
                                                                                && s.SlotID == existedSlot.SlotID 
                                                                                && s.ClassID == existedClass.ClassID 
                                                                                && !s.IsDeleted).FirstOrDefaultAsync();
                    if(existedSchedule is not null)
                    {
                        errors.Add($"A schedule already exists for class {newEntity.ClassCode} at slot {newEntity.SlotNumber} on date {newEntity.Date}");
                        continue;
                    }

                    var conflictingSchedule = await _unitOfWork.ScheduleRepository.Get(
                                                                               s => s.Date == newEntity.Date 
                                                                            && s.SlotID == existedSlot.SlotID
                                                                            && s.ClassID != existedClass.ClassID
                                                                            && s.Class!.RoomID == existedClass.RoomID 
                                                                            && !s.IsDeleted).ToArrayAsync();
                    if( conflictingSchedule.Count() > 0)
                    {
                        errors.Add($"Another class is scheduled with the same room, slot on date '{newEntity.Date}'.");
                        continue;
                    }

                    var conflictingScheduleLecturer = 
                    await _unitOfWork.ScheduleRepository.Get(s => s.Date == newEntity.Date 
                                                            && s.SlotID == existedSlot.SlotID
                                                            && s.ClassID != existedClass.ClassID 
                                                            && s.Class!.RoomID == existedClass.RoomID
                                                            && s.Class!.LecturerID == existedClass.LecturerID && !s.IsDeleted).ToArrayAsync();
                    if (conflictingScheduleLecturer.Count() > 0)
                    {
                        errors.Add($"Lecturer already have class for this slot");
                        continue;
                    }

                    DayOfWeek dayOfWeek = newEntity.Date.DayOfWeek;
                    Schedule newSchedule = new Schedule()
                    {
                        Date = newEntity.Date,
                        DateOfWeek =(int)dayOfWeek,
                        ScheduleStatus = 1,
                        SlotID = existedSlot.SlotID,
                        ClassID = existedClass.ClassID,
                        CreatedBy = _currentUserService.UserId,
                        CreatedAt = ServerDateTime.GetVnDateTime(),
                        IsDeleted = false
                    };
                        await _unitOfWork.ScheduleRepository.AddAsync(newSchedule);
                        createdSchedule.Add(newEntity);
            }

            if (errors.Count > 0)
            {
                return new ServiceResponseVM<List<ScheduleVM>>
                {
                    IsSuccess = false,
                    Title = "Create schedule failed",
                    Errors = errors.Distinct().ToArray()
                };
            }

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponseVM<List<ScheduleVM>>
                    {
                        IsSuccess = true,
                        Title = "Create schedule successfully",
                        Result = createdSchedule
                    };
                }
                else
                {
                    return new ServiceResponseVM<List<ScheduleVM>>
                    {
                        IsSuccess = false,
                        Title = "Create schedule failed",
                        Errors = errors.Distinct().ToArray()
                    };
                }

            }
            catch (DbUpdateException ex)
            {
                errors.Add($"DbUpdateException: {ex.Message}");
                return new ServiceResponseVM<List<ScheduleVM>>
                {
                    IsSuccess = false,
                    Title = "Create schedule failed",
                    Errors = errors.Distinct().ToArray()
                };
            }
            catch (OperationCanceledException ex)
            {
                errors.Add($"OperationCanceledException: {ex.Message}");
                return new ServiceResponseVM<List<ScheduleVM>>
                {
                    IsSuccess = false,
                    Title = "Create schedule failed",
                    Errors = errors.Distinct().ToArray()
                };
            }
        }

        public async Task<IEnumerable<Schedule>> GetSchedules(int startPage, int endPage, Guid lecturerId, int quantity, int? semesterId, DateTime? startDate, DateTime? endDate)
        {
            int quantityResult = 0;
            _validateGet.ValidateGetRequest(ref startPage, ref endPage, quantity, ref quantityResult);
            if (quantityResult == 0)
            {
                throw new ArgumentException("Error when get quantity per page");
            }

            var query = await _unitOfWork.ScheduleRepository.Get(s => s.Class!.LecturerID == lecturerId && !s.IsDeleted)
                .Include(s => s.Class)
                .Include(s => s.Room)
                .Include(s => s.Class!.Semester)
                .Include(s => s.Class!.Room)
                .Include(s => s.Slot)
                .Include(s => s.Attendances)
                .Include(s => s.Class!.Subject).ToArrayAsync();

            if(semesterId.HasValue)
            {
                query = query.Where(s => s.Class!.SemesterID == semesterId).ToArray();
            }

            if (startDate.HasValue)
            {
                var startDateOnly = DateOnly.FromDateTime(startDate.Value);
                query = query.Where(s => s.Date >= startDateOnly).ToArray();

            }
            if (endDate.HasValue)
            {
                var endDateOnly = DateOnly.FromDateTime(endDate.Value);
                query =  query.Where(s => s.Date <= endDateOnly).ToArray();
            }
            
            var schedules = query
                .OrderBy(s => s.Slot!.Order)
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult);

            foreach (var schedule in schedules)
            {
                var attendanceCount = schedule.Attendances.Count(a => a.AttendanceStatus == 1);
                var attendancePerClass = _unitOfWork.AttendanceRepository.Get(a => a.ScheduleID == schedule.ScheduleID).Count();
                
                schedule.AttendStudent = attendanceCount.ToString() +"/"+ attendancePerClass.ToString();
            }

            return schedules;
        }

        public async Task<Schedule?> GetById(int scheduleId)
        {
            var includes = new Expression<Func<Schedule, object?>>[]
            {
                s => s.Slot,
                s => s.Class,
                s => s.Room,
                s => s.Attendances
            };
            return await _unitOfWork.ScheduleRepository
                .Get(s => s.ScheduleID == scheduleId, includes)
                .Include(nameof(Schedule.Attendances) + "." + nameof(Attendance.Student.Student))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public string? GetClassCodeList(string deliminate, List<int>? scheduleIds)
        {
            if(scheduleIds is null || scheduleIds.Count() <= 0)
            {
                return null;
            }
            var includes = new Expression<Func<Schedule, object?>>[]
            {
                s => s.Class
            };
            var classCodeList = _unitOfWork.ScheduleRepository
                .Get(s => scheduleIds.Any(id => id == s.ScheduleID), includes)
                .Select(s => s.Class!.ClassCode)
                .ToList();
            if (classCodeList is null || classCodeList.Count() == 0) return null;
            if (classCodeList.Count() == 1) return classCodeList.FirstOrDefault();

            return String.Join(deliminate, classCodeList);
        }

        public async Task<ServiceResponseVM<IEnumerable<Schedule>>> GetAllSchedules(int startPage, int endPage, int quantity, Guid? lecturerId, int? semesterId, DateOnly? startDate, DateOnly? endDate, IEnumerable<int> scheduleIds)
        {
            var result = new ServiceResponseVM<IEnumerable<Schedule>>()
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

            MethodInfo? containsMethod = typeof(List<int>).GetMethod("Contains", new[] { typeof(int) });

            if (containsMethod is null)
            {
                errors.Add("Method Contains can not found from list type");
                result.Errors = errors;
                return result;
            }

            var expressions = new List<Expression>();
            ParameterExpression pe = Expression.Parameter(typeof(Schedule), "s");

            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Schedule.IsDeleted)), Expression.Constant(false)));

            if(lecturerId is not null)
            {
                var classProperty = Expression.Property(pe, "Class");
                var employeeIdProperty = Expression.Property(classProperty, "LecturerID");
                expressions.Add(Expression.Equal(employeeIdProperty, Expression.Constant(lecturerId)));
            }

            if(semesterId is not null)
            {
                var classProperty = Expression.Property(pe, "Class");
                var semesterIdProperty = Expression.Property(classProperty, "SemesterID");
                expressions.Add(Expression.Equal(semesterIdProperty, Expression.Constant(semesterId)));
            }

            if(startDate is not null)
            {
                expressions.Add(Expression.GreaterThanOrEqual(Expression.Property(pe, nameof(Schedule.Date)), Expression.Constant(startDate)));
            }

            if(endDate is not null)
            {
                expressions.Add(Expression.LessThanOrEqual(Expression.Property(pe, nameof(Schedule.Date)), Expression.Constant(endDate)));
            }

            if(scheduleIds != Enumerable.Empty<int>() && scheduleIds.Count() > 0)
            {
                expressions.Add(Expression.Call(Expression.Constant(scheduleIds.ToList()), containsMethod, Expression.Property(pe, nameof(Schedule.ScheduleID))));
            }

            Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
            Expression<Func<Schedule, bool>> where = Expression.Lambda<Func<Schedule, bool>>(combined, pe);

            var includes = new Expression<Func<Schedule, object?>>[]
            {
                s => s.Class,
                s => s.Slot
            };

            var schedules = await _unitOfWork.ScheduleRepository
                .Get(where, includes)
                .OrderBy(s => s.Slot!.Order)
                .AsNoTracking()
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult)
                .ToArrayAsync();

            result.IsSuccess = true;
            result.Result = schedules;
            result.Title = "Get successfully";

            return result;
        }

        public async Task<ImportServiceResposneVM<Schedule>> ImportSchedule(List<Schedule> schedules, int semesterId, Guid userID, DateOnly importStartDate, DateOnly importEndDate)
        {
            var result = new ImportServiceResposneVM<Schedule>()
            {
                IsSuccess = false
            };
            var errors = new List<string>();

            // Create import record
            Guid creatorId;
            if (!Guid.TryParse(_currentUserService.UserId, out creatorId))
            {
                result.Title = "Import schedules failed";
                result.IsSuccess = false;
                result.Errors = new string[1] { "User not found" };
                return result;
            }
            var existedUser = _unitOfWork.UserRepository.Get(u => u.Id == creatorId).AsNoTracking().FirstOrDefault();
            if(existedUser is null)
            {
                result.Title = "Import schedules failed";
                result.IsSuccess = false;
                result.Errors = new string[1] { "User not found" };
                return result;
            }
            var newRecord = new ImportSchedulesRecord
            {
                RecordTimestamp = ServerDateTime.GetVnDateTime(),
                ImportReverted = false,
                IsReversible = true,
                UserId = creatorId
            };

            if (schedules.Count == 0)
            {
                errors.Add("No schedules found");
            }

            var existedSemester = _unitOfWork.SemesterRepository.Get(s => !s.IsDeleted && s.SemesterID == semesterId).FirstOrDefault();
            if(existedSemester is null)
            {
                errors.Add("Semester not found");
            }

            if(errors.Count > 0)
            {
                result.Title = "Import schedules failed";
                result.IsSuccess = false;
                result.Errors = errors;
                return result;
            }

            if (importStartDate < existedSemester!.StartDate)
            {
                errors.Add("Start date must be greater than or equal to " + existedSemester!.StartDate.ToString("dd-MM-yyyy"));
            }
            if (importEndDate > existedSemester!.EndDate)
            {
                errors.Add("End date must be less than or equal to " + existedSemester!.EndDate.ToString("dd-MM-yyyy"));
            }
            if (errors.Count > 0)
            {
                result.Title = "Import schedules failed";
                result.IsSuccess = false;
                result.Errors = errors;
                return result;
            }


            // This data is for limiting the requests to database
            List<Slot> slots = await _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted)
                .AsNoTracking()
                .ToListAsync();
            List<Class> classes = await _unitOfWork.ClassRepository
                .Get(c => !c.IsDeleted && c.SemesterID == semesterId && c.LecturerID == userID)
                .AsNoTracking()
                .ToListAsync();

            ConcurrentBag<Schedule> importedSchedules = new ConcurrentBag<Schedule>();
            ConcurrentBag<ImportErrorEntity<Schedule>> errorSchedules = new ConcurrentBag<ImportErrorEntity<Schedule>>();

            // Lets filter schedules have correct class code and slot information
            // Verify the scheduling date is valid or not
            var startDate = existedSemester?.StartDate ?? new DateOnly();
            var endDate = existedSemester?.EndDate ?? new DateOnly();
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.4 * 2))
            };
            Parallel.ForEach(schedules, parallelOptions, (schedule, state) =>
            {
                var errors = new List<string>();

                var matchedClass = classes.FirstOrDefault(c => c.ClassCode.ToUpper() == schedule.Class?.ClassCode.ToUpper());

                Slot? matchedSlot = null;

                if (matchedClass is null)
                {
                    errors.Add("Class code " + schedule.Class?.ClassCode + " not found");
                }
                else
                {
                    matchedSlot = slots.FirstOrDefault(s => s.SlotNumber == schedule.Slot?.SlotNumber && s.SlotTypeId == matchedClass.SlotTypeId);

                    if (matchedSlot is null)
                    {
                        errors.Add("Slot " + schedule.Slot?.SlotNumber + " not found");
                    }
                }
                
                if(schedule.Date < startDate || schedule.Date > endDate)
                {
                    errors.Add("The scheduling date is not belong to semester " + existedSemester?.SemesterCode);
                }

                if(errors.Count > 0)
                {
                    errorSchedules.Add(new ImportErrorEntity<Schedule>
                    {
                        ErrorEntity = schedule,
                        Errors = errors
                    });
                }
                else
                {
                    schedule.SlotID = matchedSlot?.SlotID ?? 0;
                    schedule.ClassID = matchedClass?.ClassID ?? 0;
                    importedSchedules.Add(schedule);
                }
            });
            if(importedSchedules.Count == 0)
            {
                Parallel.ForEach(errorSchedules, parallelOptions, (errorEntity, state) =>
                {
                    if (errorEntity.ErrorEntity is not null)
                    {
                        errorEntity.ErrorEntity.Slot = slots.Where(s => s.SlotID == errorEntity.ErrorEntity.SlotID).FirstOrDefault();
                        errorEntity.ErrorEntity.Class = classes.Where(s => s.ClassID == errorEntity.ErrorEntity.ClassID).FirstOrDefault();
                    }
                });

                result.Title = "Import schedules failed";
                result.IsSuccess = false;
                result.Errors = new List<string> { "Invalid schedules" };
                result.ErrorEntities = errorSchedules.ToList();
                return result;
            }


            // Lets handle import schedules in a time frame
            var noDuplicatedSchedules = importedSchedules.ToList();
            Parallel.ForEach(noDuplicatedSchedules, parallelOptions, (schedule, state) =>
            {
                bool check = true;
                var date = schedule.Date;

                Schedule duplicateSchedule = new Schedule();
                duplicateSchedule.ScheduleStatus = schedule.ScheduleStatus;
                duplicateSchedule.SlotID = schedule.SlotID;
                duplicateSchedule.ClassID = schedule.ClassID;
                duplicateSchedule.RoomID = schedule.RoomID;
                duplicateSchedule.CreatedAt = schedule.CreatedAt;
                duplicateSchedule.CreatedBy = schedule.CreatedBy;

                while (check)
                {
                    var copySchedule = (Schedule)duplicateSchedule.Clone();

                    date = date.AddDays(7);
                    if (date > importEndDate)
                    {
                        check = false;
                    }
                    else
                    {
                        copySchedule.Date = date;
                        copySchedule.DateOfWeek = (int)date.DayOfWeek;
                        importedSchedules.Add(copySchedule);
                    }
                }
            });
            Parallel.ForEach(noDuplicatedSchedules, parallelOptions, (schedule, state) =>
            {
                bool check = true;
                var date = schedule.Date;

                Schedule duplicateSchedule = new Schedule();
                duplicateSchedule.ScheduleStatus = schedule.ScheduleStatus;
                duplicateSchedule.SlotID = schedule.SlotID;
                duplicateSchedule.ClassID = schedule.ClassID;
                duplicateSchedule.RoomID = schedule.RoomID;
                duplicateSchedule.CreatedAt = schedule.CreatedAt;
                duplicateSchedule.CreatedBy = schedule.CreatedBy;

                while (check)
                {
                    var copySchedule = (Schedule)duplicateSchedule.Clone();

                    date = date.AddDays(-7);
                    if (date < importStartDate)
                    {
                        check = false;
                    }
                    else
                    {
                        copySchedule.Date = date;
                        copySchedule.DateOfWeek = (int)date.DayOfWeek;
                        importedSchedules.Add(copySchedule);
                    }
                }
            });

            // Lets filter schedules within the range
            var filterSchedules = importedSchedules.ToList();
            importedSchedules = new ConcurrentBag<Schedule>();
            Parallel.ForEach(filterSchedules, parallelOptions, (schedule, state) =>
            {
                if (schedule.Date >= importStartDate && schedule.Date <= importEndDate)
                {
                    importedSchedules.Add(schedule);
                }
            });

            // Lets check whether if the schedule is already added
            var verifiedSchedules = importedSchedules.ToList();
            var copySchedules = verifiedSchedules.ToList();
            foreach(var schedule in copySchedules)
            {
                var existedSchedule = await _unitOfWork.ScheduleRepository
                    .Get(s => !s.IsDeleted && 
                        s.Date == schedule.Date && 
                        s.ClassID == schedule.ClassID && 
                        s.SlotID == schedule.SlotID)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                if(existedSchedule is not null)
                {
                    errorSchedules.Add(new ImportErrorEntity<Schedule>
                    {
                        ErrorEntity = schedule,
                        Errors = new List<string> { "Schedule is already added" }
                    });
                    verifiedSchedules.Remove(schedule);
                }
            }
            if (verifiedSchedules.Count == 0)
            {
                Parallel.ForEach(errorSchedules, parallelOptions, (errorEntity, state) =>
                {
                    if (errorEntity.ErrorEntity is not null)
                    {
                        errorEntity.ErrorEntity.Slot = slots.Where(s => s.SlotID == errorEntity.ErrorEntity.SlotID).FirstOrDefault();
                        errorEntity.ErrorEntity.Class = classes.Where(s => s.ClassID == errorEntity.ErrorEntity.ClassID).FirstOrDefault();
                    }
                });

                result.Title = "Import schedules failed";
                result.IsSuccess = false;
                result.Errors = new List<string> { "Invalid schedules" };
                result.ErrorEntities = errorSchedules.ToList();
                return result;
            }


            // Tại đây check overlap cho 2 kiểu luôn, nên lấy hết lịch trong time frame từ importStartDate đến importEndDate
            // (gom nhóm lại ra từng ngày cho dễ quản lý - đỡ phải iterate hết list)
            //
            // Từng lịch cần check, tìm danh sách lịch trong ngày đó ra trc (Check trùng ngày)
            // 1. -> Check trùng slot Id
            // 2. -> Lấy ra các slot bị overlap -> Check trùng timeframe
            //
            // Lets check whether if the schedule overlap the existed schedule
            var checkingScheduleGroups = _unitOfWork.ScheduleRepository
                .Get(s => !s.IsDeleted && importStartDate <= s.Date && s.Date <= importEndDate &&
                        s.Class!.LecturerID == userID && s.Class!.SemesterID == semesterId,
                     new Expression<Func<Schedule, object?>>[]
                     {
                         s => s.Class
                     })
                .AsNoTracking()
                .ToImmutableArray()
                .GroupBy(s => s.Date)
                .ToImmutableArray();

            DbContextFactory dbFactory = new DbContextFactory();
            importedSchedules = new ConcurrentBag<Schedule>();
            Parallel.ForEach(verifiedSchedules, parallelOptions, (schedule, state) =>
            {
                // Validate here
                //========================
                var slot = slots.FirstOrDefault(s => s.SlotID == schedule.SlotID);
                if(slot is not null)
                {
                    var startTime = slot.StartTime;
                    var endTime = slot.Endtime;
                    var overlapSlotIds = slots.Where(s => s.SlotTypeId != slot.SlotTypeId && 
                    (
                        (s.StartTime <= startTime && startTime <= s.Endtime) ||
                        (s.StartTime <= endTime && endTime <= s.Endtime) ||
                        (startTime <= s.StartTime && s.StartTime <= endTime)
                    ))
                    .Select(s => s.SlotID).ToList();
                    overlapSlotIds.Add(slot.SlotID);

                    var overlapSchedule = checkingScheduleGroups.FirstOrDefault(g => g.Key == schedule.Date)?
                        .Where(s => overlapSlotIds.Contains(s.SlotID))
                        .FirstOrDefault();

                    if(overlapSchedule is not null)
                    {
                        var overlapSlot = slots.FirstOrDefault(s => s.SlotID == overlapSchedule.SlotID);
                        errorSchedules.Add(new ImportErrorEntity<Schedule>
                        {
                            ErrorEntity = schedule,
                            Errors = new List<string>()
                            {
                                "There is already a class " + (overlapSchedule.Class?.ClassCode ?? "***") + " scheduled on " + overlapSchedule.Date.ToString("dd-MM-yyyy") + " at slot " + (overlapSlot?.SlotNumber.ToString() ?? "***")
                            }
                        });
                    }
                    else
                    {
                        schedule.Class = null;
                        schedule.Slot = null;
                        schedule.Room = null;
                        schedule.ImportSchedulesRecord = newRecord;
                        importedSchedules.Add(schedule);
                    }
                }
                else
                {
                    var errorClass = classes.FirstOrDefault(c => c.ClassID == schedule.ClassID);
                    errorSchedules.Add(new ImportErrorEntity<Schedule>
                    {
                        ErrorEntity = schedule,
                        Errors = new List<string>()
                            {
                                $"Unable to add schedule of class {errorClass?.ClassCode ?? "***"} on {schedule.Date}"
                            }
                    });
                }
            });
            if (importedSchedules.Count == 0)
            {
                Parallel.ForEach(errorSchedules, parallelOptions, (errorEntity, state) =>
                {
                    if (errorEntity.ErrorEntity is not null)
                    {
                        errorEntity.ErrorEntity.Slot = slots.Where(s => s.SlotID == errorEntity.ErrorEntity.SlotID).FirstOrDefault();
                        errorEntity.ErrorEntity.Class = classes.Where(s => s.ClassID == errorEntity.ErrorEntity.ClassID).FirstOrDefault();
                    }
                });

                result.Title = "Import schedules failed";
                result.IsSuccess = false;
                result.Errors = new List<string> { "Invalid schedules" };
                result.ErrorEntities = errorSchedules.ToList();
                return result;
            }


            var currentDateTime = ServerDateTime.GetVnDateTime();
            var currentDateOnly = DateOnly.FromDateTime(currentDateTime);
            var currentTimeOnly = TimeOnly.FromDateTime(currentDateTime);
            var currentSlot = slots.Where(s => s.StartTime <= currentTimeOnly && s.Endtime >= currentTimeOnly)
                .Select(s => s.SlotID)
                .FirstOrDefault();
            var pastSlot = slots.Where(s => s.StartTime > currentTimeOnly)
                .Select(s => s.SlotID);
            var futureSlot = slots.Where(s => s.Endtime < currentTimeOnly)
                .Select(s => s.SlotID);
            Parallel.ForEach(importedSchedules, parallelOptions, (schedule, state) =>
            {
                if (schedule.Date == currentDateOnly)
                {
                    if (schedule.SlotID == currentSlot)
                    {
                        schedule.ScheduleStatus = 2;
                    }
                    else if (futureSlot.Any(s => s == schedule.SlotID))
                    {
                        schedule.ScheduleStatus = 1;
                    }
                    else if (pastSlot.Any(s => s == schedule.SlotID))
                    {
                        schedule.ScheduleStatus = 3;
                    }
                }
                else if (schedule.Date > currentDateOnly)
                {
                    schedule.ScheduleStatus = 1;
                }
                else if (schedule.Date < currentDateOnly)
                {
                    schedule.ScheduleStatus = 3;
                }
            });


            // Lets create schedules
            var createSchedules = importedSchedules.ToList();
            newRecord.Title = "Imported " + createSchedules.Count() + " schedules from " + importStartDate.ToString("dd-MM-yyyy") + " to " + importEndDate.ToString("dd-MM-yyyy");
            try
            {
                await _unitOfWork.ImportSchedulesRecordRepository.AddAsync(newRecord);
                await _unitOfWork.ScheduleRepository.AddRangeAsync(createSchedules);
                var saveResult = await _unitOfWork.SaveChangesAsync();
                if (!saveResult)
                {
                    result.IsSuccess = false;
                    result.Title = "Import schedule failed";
                    result.Errors = new List<string> { "Error when saving changes" };
                    return result;
                }
            }
            catch(Exception ex)
            {
                result.IsSuccess = false;
                result.Title = "Import schedule failed";
                result.Errors = new List<string> { "Error when saving changes", ex.Message };
                return result;
            }

            // Set hangfire to make record unable to revert after the duration
            var recordId = newRecord.ImportSchedulesRecordID;
            var revertableDuration = _unitOfWork.SystemConfigurationRepository.Get(s => true).FirstOrDefault()?.RevertableDurationInHours ?? 12;
            var recordTimeStamp = newRecord.RecordTimestamp;
            var endTimeStamp = recordTimeStamp.AddHours(revertableDuration);
            _hangfireService.SetRecordIrreversible(recordId, endTimeStamp);

            // Set class and slot here
            var task1 = Task.Run(() => Parallel.ForEach(createSchedules, parallelOptions, (schedule, state) =>
            {
                schedule.Slot = slots.Where(s => s.SlotID == schedule.SlotID).FirstOrDefault();
                schedule.Class = classes.Where(s => s.ClassID == schedule.ClassID).FirstOrDefault();
            }));
            var task2 = Task.Run(() => Parallel.ForEach(errorSchedules, parallelOptions, (errorEntity, state) =>
            {
                if(errorEntity.ErrorEntity is not null)
                {
                    errorEntity.ErrorEntity.Slot = slots.Where(s => s.SlotID == errorEntity.ErrorEntity.SlotID).FirstOrDefault();
                    errorEntity.ErrorEntity.Class = classes.Where(s => s.ClassID == errorEntity.ErrorEntity.ClassID).FirstOrDefault();
                }
            }));
            await Task.WhenAll(task1, task2);

            result.IsSuccess = true;
            result.Title = "Import schedules successfully";
            result.ImportedEntities = createSchedules;
            result.ErrorEntities = errorSchedules.ToList();

            return result;
        }

        public async Task<ServiceResponseVM<Schedule>> CreateNewSchedule(CreateScheduleVM resource)
        {
            var result = new ServiceResponseVM<Schedule> 
            {
                IsSuccess = false
            };

            var existedSlot = _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted && s.SlotID == resource.SlotId,
                new Expression<Func<Slot, object?>>[]
                {
                    s => s.SlotType
                })
                .AsNoTracking()
                .FirstOrDefault();
            if(existedSlot is null)
            {
                result.IsSuccess = false;
                result.Title = "Create new schedule failed";
                result.Errors = new string[1] { "Slot not found" };
                return result;
            }

            var existedClass = _unitOfWork.ClassRepository
                .Get(c => !c.IsDeleted && c.ClassID == resource.ClassId)
                .AsNoTracking()
                .FirstOrDefault();
            if(existedClass is null)
            {
                result.IsSuccess = false;
                result.Title = "Create new schedule failed";
                result.Errors = new string[1] { "Class not found" };
                return result;
            }

            // Validate slot type
            if(existedSlot.SlotTypeId != existedClass.SlotTypeId)
            {
                var errors = new List<string>()
                {
                    "Slot type is not compatible"
                };
                var requiredSlotType = await _unitOfWork.SlotTypeRepository.FindAsync(existedClass.SlotTypeId);
                if(requiredSlotType is not null)
                {
                    errors.Add($"{requiredSlotType.SessionCount}-sessions slot type is mandatory");
                }

                result.IsSuccess = false;
                result.Title = "Create new schedule failed";
                result.Errors = errors;
                return result;
            }

            var existedSemester = _unitOfWork.SemesterRepository
                .Get(s => !s.IsDeleted && s.SemesterID == existedClass.SemesterID)
                .AsNoTracking()
                .FirstOrDefault();
            if(existedSemester is null)
            {
                result.IsSuccess = false;
                result.Title = "Create new schedule failed";
                result.Errors = new string[1] { "The schedule is not within the range of semester" };
                return result;
            }
            if(resource.Date < existedSemester.StartDate || resource.Date > existedSemester.EndDate)
            {
                result.IsSuccess = false;
                result.Title = "Create new schedule failed";
                result.Errors = new string[1] { "The schedule is not within the range of semester " + existedSemester.SemesterCode };
                return result;
            }

            if(resource.RoomId is not null)
            {
                var existedRoom = _unitOfWork.RoomRepository.Get(r => !r.IsDeleted && r.RoomID == resource.RoomId).AsNoTracking().FirstOrDefault();
                if(existedRoom is null)
                {
                    result.IsSuccess = false;
                    result.Title = "Create new schedule failed";
                    result.Errors = new string[1] { "Room not found" };
                    return result;
                }
            }

            var checkAlreadCreateSchedule = _unitOfWork.ScheduleRepository
                .Get(s => !s.IsDeleted && s.ClassID == resource.ClassId && s.SlotID == resource.SlotId && s.Date == resource.Date)
                .AsNoTracking()
                .FirstOrDefault();
            if(checkAlreadCreateSchedule is not null)
            {
                result.IsSuccess = false;
                result.Title = "Create new schedule failed";
                result.Errors = new string[1] { "Schedule is already added" };
                return result;
            }
            
            // Validate slot step 1
            // Check whether if any other schedules of the lecturer on the same date and used the same slot id
            var checkOverlapSchedule = _unitOfWork.ScheduleRepository
                .Get(s => !s.IsDeleted && s.Date == resource.Date && s.SlotID == resource.SlotId && s.Class!.LecturerID == existedClass.LecturerID,
                    new Expression<Func<Schedule, object?>>[]
                    {
                        s => s.Class,
                        s => s.Slot
                    })
                .AsNoTracking()
                .FirstOrDefault();
            if(checkOverlapSchedule is not null)
            {
                result.IsSuccess = false;
                result.Title = "Create new schedule failed";
                result.Errors = new string[1] { "There is already a class " + checkOverlapSchedule.Class?.ClassCode + " scheduled on " + checkOverlapSchedule.Date.ToString("dd-MM-yyyy") + " at " + checkOverlapSchedule.Slot?.StartTime.ToString("hh:mm:ss") + "-" + checkOverlapSchedule.Slot?.Endtime.ToString("hh:mm:ss") };
                return result;
            }

            // Validate slot step 2
            // Validate the timeframe of slot
            var startTime = existedSlot.StartTime;
            var endTime = existedSlot.Endtime;
            var overlapSlotIds = _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted && s.SlotTypeId != existedSlot.SlotTypeId &&
                    (   
                        (s.StartTime <= startTime && startTime <= s.Endtime) || 
                        (s.StartTime <= endTime && endTime <= s.Endtime) ||
                        (startTime <= s.StartTime && s.StartTime <= endTime)
                    ))
                .AsNoTracking()
                .Select(s => s.SlotID)
                .ToList();
            if(overlapSlotIds is not null && overlapSlotIds.Count() > 0)
            {
                var otherOverlapSchedules = _unitOfWork.ScheduleRepository
                    .Get(s => !s.IsDeleted && s.Date == resource.Date && 
                        s.Class!.LecturerID == existedClass.LecturerID && overlapSlotIds.Contains(s.SlotID),
                        new Expression<Func<Schedule, object?>>[]
                        {
                            s => s.Class,
                            s => s.Slot
                        })
                    .AsNoTracking()
                    .ToList();
                if(otherOverlapSchedules is not null && otherOverlapSchedules.Count() > 0)
                {
                    var errors = new List<string>();
                    foreach(var schedule in otherOverlapSchedules)
                    {
                        errors.Add("There is already a class " + schedule.Class?.ClassCode + 
                            " scheduled on " + schedule.Date.ToString("dd-MM-yyyy") + 
                            " at " + schedule.Slot?.StartTime.ToString("hh:mm:ss") + "-" + schedule.Slot?.Endtime.ToString("hh:mm:ss"));
                    }

                    result.IsSuccess = false;
                    result.Title = "Create new schedule failed";
                    result.Errors = errors;
                    return result;
                }
            }

            var createdSchedule = new Schedule
            {
                Date = resource.Date,
                DateOfWeek = (int)resource.Date.DayOfWeek,
                SlotID = resource.SlotId,
                ClassID = resource.ClassId,
                RoomID = resource.RoomId
            };

            var currentDate = ServerDateTime.GetVnDateTime();
            var currentDateOnly = DateOnly.FromDateTime(currentDate);
            var currentTimeOnly = TimeOnly.FromDateTime(currentDate);
            if (createdSchedule.Date == currentDateOnly)
            {
                if(existedSlot.StartTime <= currentTimeOnly && existedSlot.Endtime >= currentTimeOnly)
                {
                    createdSchedule.ScheduleStatus = 2;
                }
                else if(existedSlot.StartTime > currentTimeOnly)
                {
                    createdSchedule.ScheduleStatus = 1;
                }
                else if (existedSlot.Endtime < currentTimeOnly)
                {
                    createdSchedule.ScheduleStatus = 3;
                }
            }
            else if(createdSchedule.Date > currentDateOnly)
            {
                createdSchedule.ScheduleStatus = 1;
            }
            else if (createdSchedule.Date < currentDateOnly)
            {
                createdSchedule.ScheduleStatus = 3;
            }

            try
            {
                await _unitOfWork.ScheduleRepository.AddAsync(createdSchedule);
                var saveChangesResult = await _unitOfWork.SaveChangesAsync();
                if (saveChangesResult)
                {
                    result.IsSuccess = true;
                    result.Title = "Create new schedule successfully";
                    result.Result = createdSchedule;
                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Title = "Create new schedule failed";
                    result.Errors = new string[1] { "Error when saving changes" };
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Title = "Create new schedule failed";
                result.Errors = new List<string> { "Error when saving changes", ex.Message };
                return result;
            }
        }

        public async Task<ServiceResponseVM> DeleteSchedules(DeleteSchedulesVM resource)
        {
            var result = new ServiceResponseVM
            {
                IsSuccess = false
            };

            var deletedSchedules = _unitOfWork.ScheduleRepository
                .Get(s => !s.IsDeleted && s.Date >= resource.StartDate && s.Date <= resource.EndDate && s.Class!.LecturerID == resource.UserID,
                new Expression<Func<Schedule, object?>>[]
                {
                    s => s.Attendances,
                });

            if(resource.SlotIDs.Count() > 0)
            {
                deletedSchedules = deletedSchedules.Where(s => resource.SlotIDs.Any(i => i == s.SlotID));
            }

            var schedules = deletedSchedules.ToArray();

            if(schedules.Count() == 0)
            {
                result.IsSuccess = true;
                result.Title = "Delete schedules successfully";
                return result;
            }

            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling(Environment.ProcessorCount * 0.4 * 2))
            };
            Parallel.ForEach(schedules, parallelOptions, (schedule, state) =>
            {
                schedule.IsDeleted = true;
                foreach (var attendance in schedule.Attendances)
                {
                    attendance.IsDeleted = true;
                }
            });

            try
            {
                var saveChangesResult = await _unitOfWork.SaveChangesAsync();
                if (saveChangesResult)
                {
                    result.IsSuccess = true;
                    result.Title = "Delete schedules successfully";
                    return result;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Title = "Delete schedules failed";
                    result.Errors = new List<string> { "Error when saving changes" };
                    return result;
                }
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Title = "Delete schedules failed";
                result.Errors = new List<string> { "Error when saving changes", ex.Message };
                return result;
            }
        }

        public async Task<Schedule?> GetByIdForModule(int scheduleId)
        {
            var includes = new Expression<Func<Schedule, object?>>[]
            {
                s => s.Slot,
                s => s.Class,
            };
            return await _unitOfWork.ScheduleRepository
                .Get(s => s.ScheduleID == scheduleId, includes)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<ServiceResponseVM> DeleteById(int id)
        {
            var existedSchedule = _unitOfWork.ScheduleRepository
                .Get(s => !s.IsDeleted && s.ScheduleID == id,
                new Expression<Func<Schedule, object?>>[]
                {
                    s => s.Attendances
                })
                .FirstOrDefault();

            if(existedSchedule is null)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete schedule failed",
                    Errors = new string[1] { "Schedule not found" }
                };
            }

            if(existedSchedule.ScheduleStatus == 2)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete schedule failed",
                    Errors = new string[1] { "The schedule is already in progress" }
                };
            }

            if(existedSchedule.ScheduleStatus == 3)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete schedule failed",
                    Errors = new string[1] { "The schedule is already ended" }
                };
            }

            existedSchedule.IsDeleted = true;
            foreach (var attendance in existedSchedule.Attendances)
            {
                attendance.IsDeleted = true;
            }

            try
            {
                var saveChangesResult = await _unitOfWork.SaveChangesAsync();
                if (saveChangesResult)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = true,
                        Title = "Delete schedule successfully",
                    };
                }
                else
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Title = "Delete schedule failed",
                        Errors = new string[1] { "Error when saving changes" }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete schedule failed",
                    Errors = new string[2] { "Error when saving changes", ex.Message }
                };
            }
        }

        public async Task<ServiceResponseVM<Schedule>> UpdateSchedule(UpdateScheduleVM resource, int id)
        {
            var existedSchedule = _unitOfWork.ScheduleRepository
                .Get(s => !s.IsDeleted && s.ScheduleID == id,
                new Expression<Func<Schedule, object?>>[]
                {
                    s => s.Class
                })
                .FirstOrDefault();

            if (resource.SlotId is null && resource.RoomId is null && resource.Date is null)
            {
                return new ServiceResponseVM<Schedule>
                {
                    IsSuccess = true,
                    Title = "Update schedule successfully",
                    Result = existedSchedule
                };
            }

            if (existedSchedule is null)
            {
                return new ServiceResponseVM<Schedule>
                {
                    IsSuccess = false,
                    Title = "Update schedule failed",
                    Errors = new string[1] { "Schedule not found" }
                };
            }

            if ((resource.SlotId != null && existedSchedule.SlotID != resource.SlotId) || (resource.Date != null && existedSchedule.Date != resource.Date))
            {
                if (existedSchedule.ScheduleStatus == 2)
                {
                    return new ServiceResponseVM<Schedule>
                    {
                        IsSuccess = false,
                        Title = "Update schedule failed",
                        Errors = new string[1] { "The schedule is already in progress" }
                    };
                }

                if (existedSchedule.ScheduleStatus == 3)
                {
                    return new ServiceResponseVM<Schedule>
                    {
                        IsSuccess = false,
                        Title = "Update schedule failed",
                        Errors = new string[1] { "The schedule is already ended" }
                    };
                }
            }

            var copySchedule = (Schedule)existedSchedule.Clone();

            if(resource.RoomId is not null)
            {
                var existedRoom = _unitOfWork.RoomRepository
                    .Get(r => !r.IsDeleted && r.RoomID == resource.RoomId)
                    .AsNoTracking()
                    .FirstOrDefault();
                if(existedRoom is null)
                {
                    return new ServiceResponseVM<Schedule>
                    {
                        IsSuccess = false,
                        Title = "Update schedule failed",
                        Errors = new string[1] { "Room not found" }
                    };
                }
                else
                {
                    existedSchedule.RoomID = resource.RoomId;
                }
            }

            if(resource.SlotId is not null)
            {
                var existedSlot = _unitOfWork.SlotRepository
                    .Get(s => !s.IsDeleted && s.SlotID == resource.SlotId)
                    .AsNoTracking()
                    .FirstOrDefault();
                if(existedSlot is null)
                {
                    return new ServiceResponseVM<Schedule>
                    {
                        IsSuccess = false,
                        Title = "Update schedule failed",
                        Errors = new string[1] { "Slot not found" }
                    };
                }
                else
                {
                    if (existedSlot.SlotTypeId != existedSchedule.Class!.SlotTypeId)
                    {
                        var errors = new List<string>()
                        {
                            "Slot type is not compatible"
                        };
                        var requiredSlotType = await _unitOfWork.SlotTypeRepository
                            .Get(s => !s.IsDeleted && s.SlotTypeID ==  existedSchedule.Class.SlotTypeId)
                            .AsNoTracking()
                            .FirstOrDefaultAsync();
                        if (requiredSlotType is not null)
                        {
                            errors.Add($"{requiredSlotType.SessionCount}-sessions slot type is mandatory");
                        }

                        return new ServiceResponseVM<Schedule>()
                        {
                            IsSuccess = false,
                            Title = "Update schedule failed",
                            Errors = errors
                        };
                    }
                    existedSchedule.SlotID = (int)resource.SlotId;
                }
            }

            if(resource.Date is not null)
            {
                var existedSemester = _unitOfWork.SemesterRepository
                    .Get(s => !s.IsDeleted && s.SemesterID == existedSchedule.Class!.SemesterID)
                    .AsNoTracking()
                    .FirstOrDefault();
                if(existedSemester is null)
                {
                    return new ServiceResponseVM<Schedule>
                    {
                        IsSuccess = false,
                        Title = "Update schedule failed",
                        Errors = new string[1] { "The schedule is not within the range of semester" }
                    };
                }
                if (resource.Date < existedSemester.StartDate || resource.Date > existedSemester.EndDate)
                {
                    return new ServiceResponseVM<Schedule>
                    {
                        IsSuccess = false,
                        Title = "Update schedule failed",
                        Errors = new string[1] { "The schedule is not within the range of the semester " + existedSemester.SemesterCode }
                    };
                }

                existedSchedule.Date = resource.Date.Value;
            }

            if(resource.SlotId is not null || resource.Date is not null)
            {
                var checkOverlapSchedule = _unitOfWork.ScheduleRepository
                    .Get(s => !s.IsDeleted && s.ScheduleID != existedSchedule.ScheduleID &&
                        s.Date == resource.Date && s.SlotID == existedSchedule.SlotID &&
                        s.Class!.LecturerID == existedSchedule.Class!.LecturerID,
                    new Expression<Func<Schedule, object?>>[]
                    {
                        s => s.Class,
                        s => s.Slot
                    })
                    .AsNoTracking()
                    .FirstOrDefault();
                if (checkOverlapSchedule is not null)
                {
                    return new ServiceResponseVM<Schedule>
                    {
                        IsSuccess = false,
                        Title = "Update schedule failed",
                        Errors = new string[1] { "There is already a class " + checkOverlapSchedule.Class!.ClassCode + " scheduled on " + checkOverlapSchedule.Date.ToString("dd-MM-yyyy") + " at " + checkOverlapSchedule.Slot?.StartTime.ToString("hh:mm:ss") + "-" + checkOverlapSchedule.Slot?.Endtime.ToString("hh:mm:ss") }
                    };
                }

                var existedSlot = _unitOfWork.SlotRepository
                    .Get(s => !s.IsDeleted && s.SlotID == existedSchedule.SlotID)
                    .AsNoTracking()
                    .FirstOrDefault();
                if(existedSlot is not null)
                {
                    var startTime = existedSlot.StartTime;
                    var endTime = existedSlot.Endtime;
                    var overlapSlotIds = _unitOfWork.SlotRepository
                        .Get(s => !s.IsDeleted && s.SlotTypeId != existedSlot.SlotTypeId &&
                            (
                                (s.StartTime <= startTime && startTime <= s.Endtime) ||
                                (s.StartTime <= endTime && endTime <= s.Endtime) ||
                                (startTime <= s.StartTime && s.StartTime <= endTime)
                            ))
                        .AsNoTracking()
                        .Select(s => s.SlotID)
                        .ToList();
                    if (overlapSlotIds is not null && overlapSlotIds.Count() > 0)
                    {
                        var otherOverlapSchedules = _unitOfWork.ScheduleRepository
                            .Get(s => !s.IsDeleted && s.Date == resource.Date &&
                                s.Class!.LecturerID == existedSchedule.Class!.LecturerID && overlapSlotIds.Contains(s.SlotID),
                                new Expression<Func<Schedule, object?>>[]
                                {
                                    s => s.Class,
                                    s => s.Slot
                                })
                            .AsNoTracking()
                            .ToList();
                        if (otherOverlapSchedules is not null && otherOverlapSchedules.Count() > 0)
                        {
                            var errors = new List<string>();
                            foreach (var schedule in otherOverlapSchedules)
                            {
                                errors.Add("There is already a class " + schedule.Class?.ClassCode +
                                    " scheduled on " + schedule.Date.ToString("dd-MM-yyyy") +
                                    " at " + schedule.Slot?.StartTime.ToString("hh:mm:ss") + "-" + schedule.Slot?.Endtime.ToString("hh:mm:ss"));
                            }

                            return new ServiceResponseVM<Schedule>
                            {
                                IsSuccess = false,
                                Title = "Update schedule failed",
                                Errors = errors
                            };
                        }
                    }
                }
            }

            if(existedSchedule.RoomID == copySchedule.RoomID && 
                existedSchedule.SlotID == copySchedule.SlotID && 
                existedSchedule.Date == copySchedule.Date)
            {
                return new ServiceResponseVM<Schedule>
                {
                    IsSuccess = true,
                    Title = "Update schedule successfully",
                    Result = existedSchedule
                };
            }

            try
            {
                var saveChangesResult = await _unitOfWork.SaveChangesAsync();
                if (saveChangesResult)
                {
                    return new ServiceResponseVM<Schedule>
                    {
                        IsSuccess = true,
                        Title = "Update schedule successfully",
                        Result = existedSchedule
                    };
                }
                else
                {
                    return new ServiceResponseVM<Schedule>
                    {
                        IsSuccess = false,
                        Title = "Update schedule failed",
                        Errors = new string[1] { "Error when saving changes" }
                    };
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponseVM<Schedule>
                {
                    IsSuccess = false,
                    Title = "Update schedule failed",
                    Errors = new string[2] { "Error when saving changes", ex.Message }
                };
            }
        }
    }
}



