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
        public ScheduleService(IUnitOfWork unitOfWork, IValidateGet validateGet, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _validateGet = validateGet;
            _currentUserService = currentUserService;
        }

        public async Task<ServiceResponseVM<List<ScheduleVM>>> Create(List<ScheduleVM> newEntities)
        {
            List<ScheduleVM> createdSchedule = new List<ScheduleVM>();
            List<string> errors = new List<string>();
            foreach (var newEntity in newEntities)
            {
                
                    var existedClass = await _unitOfWork.ClassRepository.Get(c => c.ClassCode.Equals(newEntity.ClassCode) && !c.IsDeleted).SingleOrDefaultAsync();
                    if (existedClass is null)
                    {
                        errors.Add($"Class with Class Code: {newEntity.ClassCode} not existed");
                        continue;
                    }

                    var existedSlot = await _unitOfWork.SlotRepository.Get(s => s.SlotNumber == newEntity.SlotNumber && !s.IsDeleted).FirstOrDefaultAsync();
                    if( existedSlot is null)
                    {
                        errors.Add($"Slot {newEntity.SlotNumber} not existed");
                        continue;
                    }

                    var existedSchedule = await _unitOfWork.ScheduleRepository.Get(s => s.Date == newEntity.Date && s.SlotID == existedSlot.SlotID && !s.IsDeleted).FirstOrDefaultAsync();
                    if(existedSchedule is not null)
                    {
                        errors.Add($"A schedule already exists for class {newEntity.ClassCode} at slot {newEntity.SlotNumber} on date {newEntity.Date}");
                        continue;
                    }

                    var conflictingSchedule = await _unitOfWork.ScheduleRepository.Get(s => s.Date == newEntity.Date 
                                                                            && s.SlotID == existedSlot.SlotID && s.ClassID != existedClass.ClassID && s.Class!.RoomID == existedClass.RoomID 
                                                                            && s.ClassID == existedClass.ClassID && !s.IsDeleted).ToArrayAsync();
                    if( conflictingSchedule.Count() > 0)
                    {
                        errors.Add($"Another class is scheduled with the same room, slot on date '{newEntity.Date}'.");
                        continue;
                    }

                    var conflictingScheduleLecturer = 
                    await _unitOfWork.ScheduleRepository.Get(s => s.Date == newEntity.Date 
                                                            && s.SlotID == existedSlot.SlotID && s.ClassID != existedClass.ClassID 
                                                            && s.Class!.RoomID == existedClass.RoomID && s.ClassID == existedClass.ClassID 
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
            if(scheduleIds is null)
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

        public async Task<ServiceResponseVM<IEnumerable<Schedule>>> GetAllSchedules(int startPage, int endPage, int quantity, Guid? lecturerId, int? semesterId, DateOnly? startDate, DateOnly? endDate)
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

        public async Task<ImportServiceResposneVM<Schedule>> ImportSchedule(List<Schedule> schedules, int semesterId, Guid userID)
        {
            var result = new ImportServiceResposneVM<Schedule>()
            {
                IsSuccess = false
            };
            var errors = new List<string>();

            if(schedules.Count == 0)
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

            List<Slot> slots = await _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted)
                .ToListAsync();
            List<Class> classes = await _unitOfWork.ClassRepository
                .Get(c => !c.IsDeleted && c.SemesterID == semesterId && c.LecturerID == userID)
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
                var matchedSlot = slots.FirstOrDefault(s => s.SlotNumber == schedule.Slot?.SlotNumber);
                var matchedClass = classes.FirstOrDefault(c => c.ClassCode.ToUpper() == schedule.Class?.ClassCode.ToUpper());

                if(matchedSlot is null)
                {
                    errors.Add("Slot " + schedule.Slot?.SlotNumber + " not found");
                }
                if(matchedClass is null)
                {
                    errors.Add("Class code " + schedule.Class?.ClassCode + " not found");
                }
                if(schedule.Date < startDate || schedule.Date > endDate)
                {
                    errors.Add("The scheduling date is not associated with semester " + existedSemester?.SemesterCode);
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
                result.Title = "Import schedules failed";
                result.IsSuccess = false;
                result.Errors = new List<string> { "Invalid schedules" };
                result.ErrorEntities = errorSchedules.ToList();
                return result;
            }


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
                result.Title = "Import schedules failed";
                result.IsSuccess = false;
                result.Errors = new List<string> { "Invalid schedules" };
                result.ErrorEntities = errorSchedules.ToList();
                return result;
            }


            // Lets check whether if the schedule overlap the existed schedule
            DbContextFactory dbFactory = new DbContextFactory();
            importedSchedules = new ConcurrentBag<Schedule>();
            Parallel.ForEach(verifiedSchedules, parallelOptions, (schedule, state) =>
            {
                using (var context = dbFactory.CreateDbContext(Array.Empty<string>()))
                {
                    var overlapSchedule = context
                        .Set<Schedule>()
                        .Where(s => !s.IsDeleted &&
                            s.Date == schedule.Date &&
                            s.SlotID == schedule.SlotID &&
                            s.ClassID != schedule.ClassID)
                        .Include(s => s.Class)
                        .FirstOrDefault();
                    if(overlapSchedule is not null)
                    {
                        errorSchedules.Add(new ImportErrorEntity<Schedule>
                        {
                            ErrorEntity = schedule,
                            Errors = new List<string>() 
                            { 
                                "There is already a class " + overlapSchedule.Class?.ClassCode + " scheduled on " + overlapSchedule.Date.ToString("dd-MM-yyyy") + " at " + schedule.Slot?.StartTime.ToString("hh:mm:ss") + " - " + schedule.Slot?.Endtime.ToString("hh:mm:ss")
                            }
                        });
                    }
                    else
                    {
                        schedule.Class = null;
                        schedule.Slot = null;
                        schedule.Room = null;
                        importedSchedules.Add(schedule);
                    }
                }
            });
            if (importedSchedules.Count == 0)
            {
                result.Title = "Import schedules failed";
                result.IsSuccess = false;
                result.Errors = new List<string> { "Invalid schedules" };
                result.ErrorEntities = errorSchedules.ToList();
                return result;
            }


            // Lets create schedules
            var createSchedules = importedSchedules.ToList();
            try
            {
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

            result.IsSuccess = true;
            result.Title = "Import schedules successfully";
            result.ImportedEntities = createSchedules;
            result.ErrorEntities = errorSchedules.ToList();

            return result;
        }
    }
}
