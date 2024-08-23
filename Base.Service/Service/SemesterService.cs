using Base.IService.IService;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.RequestVM.Role;
using Base.Service.ViewModel.ResponseVM;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class SemesterService : ISemesterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidateGet _validateGet;
        public SemesterService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IValidateGet validateGet)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _validateGet = validateGet;
        }

        public async Task<ServiceResponseVM<Semester>> Create(SemesterVM newEntity)
        {
            var existedSemesterCode = await _unitOfWork.SemesterRepository
                .Get(s => !s.IsDeleted && s.SemesterCode == newEntity.SemesterCode)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (existedSemesterCode is not null)
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = false,
                    Title = "Create Semester failed",
                    Errors = new string[1] { "Semester Code is already taken" }
                };
            }

            // Validate semester duration
            if (newEntity.StartDate >= newEntity.EndDate)
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = false,
                    Title = "Create Semester failed",
                    Errors = new string[1] { "Start date must be earlier than end date" }
                };
            }
            var semesterDuration = _unitOfWork.SystemConfigurationRepository
                .Get(s => true)
                .AsNoTracking()
                .FirstOrDefault()
                ?.SemesterDurationInDays ?? 90;
            var difference = newEntity.EndDate.ToDateTime(TimeOnly.MinValue) - newEntity.StartDate.ToDateTime(TimeOnly.MinValue);
            if (difference.Days != semesterDuration)
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = false,
                    Title = "Create Semester failed",
                    Errors = new string[2] { $"The total duration of the semester is {difference.Days} days", $"The total duration of a semester must be {semesterDuration} days" }
                };
            }
            //===========================

            var overlappingSemester = await _unitOfWork.SemesterRepository.Get(s =>
                (newEntity.StartDate >= s.StartDate && newEntity.StartDate <= s.EndDate) ||
                (newEntity.EndDate >= s.StartDate && newEntity.EndDate <= s.EndDate) ||
                (s.StartDate >= newEntity.StartDate && s.StartDate <= newEntity.EndDate) ||
                (s.EndDate >= newEntity.StartDate && s.EndDate <= newEntity.EndDate)).AnyAsync();

            if (overlappingSemester)
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = false,
                    Title = "Create Semester failed",
                    Errors = new string[1] { "Overlap with existing Semester" }
                };
            }

            // Identify the semester status
            int semesterStatus = 0;
            var currentTime = DateOnly.FromDateTime(ServerDateTime.GetVnDateTime());
            if (currentTime < newEntity.StartDate)
            {
                semesterStatus = 1;
            }
            if (currentTime >= newEntity.StartDate && currentTime <= newEntity.EndDate)
            {
                semesterStatus = 2;
            }
            if(currentTime > newEntity.EndDate)
            {
                semesterStatus = 3;
            }

            Semester newSemester = new Semester
            {
                SemesterCode = newEntity.SemesterCode,
                SemesterStatus = semesterStatus,
                StartDate = newEntity.StartDate,
                EndDate = newEntity.EndDate,
                CreatedBy = _currentUserService.UserId,
                CreatedAt = ServerDateTime.GetVnDateTime(),
            };

            try
            {
                await _unitOfWork.SemesterRepository.AddAsync(newSemester);

                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponseVM<Semester>
                    {
                        IsSuccess = true,
                        Title = "Create Semester successfully",
                        Result = newSemester
                    };
                }
                else
                {
                    return new ServiceResponseVM<Semester>
                    {
                        IsSuccess = false,
                        Title = "Create Semester failed",
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = false,
                    Title = "Create Semester failed",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = false,
                    Title = "Create Semester failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }
        
        public async Task<ServiceResponseVM> Delete(int id)
        {
            var existedSemester = await _unitOfWork.SemesterRepository
                .Get(r => r.SemesterID == id && !r.IsDeleted,
                new Expression<Func<Semester, object?>>[]
                {
                    s => s.Classes.Where(c => !c.IsDeleted)
                })
                .FirstOrDefaultAsync();
            if (existedSemester is null)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete semester failed",
                    Errors = new string[1] { "Semester not found" }
                };
            }

            var currentDate = DateOnly.FromDateTime(ServerDateTime.GetVnDateTime());
            if(existedSemester.Classes.Count() > 0)
            {
                var errors = new List<string>();
                if (existedSemester.StartDate <= currentDate && existedSemester.EndDate >= currentDate)
                {
                    errors.Add("The semester is already start");
                }
                errors.Add($"There are already {existedSemester.Classes.Count()} classes in the semester");

                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete semester failed",
                    Errors = errors
                };
            }

            existedSemester.IsDeleted = true;
            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = true,
                        Title = "Delete semester successfully"
                    };
                }
                else
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Title = "Delete semester failed",
                        Errors = new string[1] { "Save changes failed" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete semester failed",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete semester failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }

        public async Task<IEnumerable<Semester>> GetSemester()
        {
            return await _unitOfWork.SemesterRepository.Get(s => s.IsDeleted == false).ToArrayAsync();
        }

        public async Task<ServiceResponseVM<Semester>> Update(SemesterVM updateSemester, int id)
        {
            var existedSemester = await _unitOfWork.SemesterRepository
                .Get(s => s.SemesterID == id && !s.IsDeleted)
                .FirstOrDefaultAsync();
            if (existedSemester is null)
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = false,
                    Title = "Update Semester failed",
                    Errors = new string[1] { "Semester not found" }
                };
            }

            if (updateSemester.SemesterCode != string.Empty && updateSemester.SemesterCode != existedSemester.SemesterCode)
            {
                var checkSemesterCode = _unitOfWork.SemesterRepository
                    .Get(s => s.SemesterCode == updateSemester.SemesterCode && !s.IsDeleted)
                    .AsNoTracking()
                    .FirstOrDefault() is not null;
                if (checkSemesterCode)
                {
                    return new ServiceResponseVM<Semester>
                    {
                        IsSuccess = false,
                        Title = "Update Semester failed",
                        Errors = new string[1] { $"Semester Code {updateSemester.SemesterCode} is already taken" }
                    };
                }
            }

            if (updateSemester.StartDate != existedSemester.StartDate || updateSemester.EndDate != existedSemester.EndDate)
            {
                // Validate semester duration
                if (updateSemester.StartDate >= updateSemester.EndDate)
                {
                    return new ServiceResponseVM<Semester>
                    {
                        IsSuccess = false,
                        Title = "Update Semester failed",
                        Errors = new string[1] { "Start date must be earlier than end date" }
                    };
                }
                var semesterDuration = _unitOfWork.SystemConfigurationRepository
                    .Get(s => true)
                    .FirstOrDefault()
                    ?.SemesterDurationInDays ?? 90;
                var difference = updateSemester.EndDate.ToDateTime(TimeOnly.MinValue) - updateSemester.StartDate.ToDateTime(TimeOnly.MinValue);
                if (difference.Days != semesterDuration)
                {
                    return new ServiceResponseVM<Semester>
                    {
                        IsSuccess = false,
                        Title = "Update Semester failed",
                        Errors = new string[2] { $"The total duration of the semester is {difference.Days} days", $"The total duration of a semester must be {semesterDuration} days" }
                    };
                }
                //===========================

                var overlappingSemester = await _unitOfWork.SemesterRepository.Get(s =>
                !s.IsDeleted && s.SemesterID != id &&
                (   
                    (updateSemester.StartDate >= s.StartDate && updateSemester.StartDate <= s.EndDate) ||
                    (updateSemester.EndDate >= s.StartDate && updateSemester.EndDate <= s.EndDate) ||
                    (s.StartDate >= updateSemester.StartDate && s.StartDate <= updateSemester.EndDate) ||
                    (s.EndDate >= updateSemester.StartDate && s.EndDate <= updateSemester.EndDate)
                )
                ).AnyAsync();

                if (overlappingSemester)
                {
                    return new ServiceResponseVM<Semester>
                    {
                        IsSuccess = false,
                        Title = "Update Semester failed",
                        Errors = new string[1] { "Overlap with existing Semester" }
                    };
                }
            }

            if (updateSemester.StartDate > existedSemester.StartDate)
            {
                var checkExistedSchedules = _unitOfWork.ScheduleRepository
                    .Get(s => !s.IsDeleted && s.Class!.SemesterID == id && s.Date >= existedSemester.StartDate && s.Date < updateSemester.StartDate)
                    .AsNoTracking()
                    .ToArray();
                if (checkExistedSchedules.Any())
                {
                    return new ServiceResponseVM<Semester>
                    {
                        IsSuccess = false,
                        Title = "Update Semester failed",
                        Errors = new string[1] { $"There are already {checkExistedSchedules.Count()} schedules scheduled from {existedSemester.StartDate.ToString("dd-MM-yyyy")} to {updateSemester.StartDate.AddDays(-1).ToString("dd-MM-yyyy")}" }
                    };
                }
            }

            if (updateSemester.EndDate < existedSemester.EndDate)
            {
                var checkExistedSchedules = _unitOfWork.ScheduleRepository
                    .Get(s => !s.IsDeleted && s.Class!.SemesterID == id && s.Date > updateSemester.EndDate && s.Date <= existedSemester.EndDate)
                    .AsNoTracking()
                    .ToArray();
                if (checkExistedSchedules.Any())
                {
                    return new ServiceResponseVM<Semester>
                    {
                        IsSuccess = false,
                        Title = "Update Semester failed",
                        Errors = new string[1] { $"There are already {checkExistedSchedules.Count()} schedules scheduled from {updateSemester.EndDate.AddDays(1).ToString("dd-MM-yyyy")} to {existedSemester.EndDate.ToString("dd-MM-yyyy")}" }
                    };
                }
            }

            existedSemester.SemesterCode = updateSemester.SemesterCode;
            existedSemester.StartDate = updateSemester.StartDate;
            existedSemester.EndDate = updateSemester.EndDate;

            // Identify semester status
            int semesterStatus = 0;
            var currentTime = DateOnly.FromDateTime(ServerDateTime.GetVnDateTime());
            if (currentTime < existedSemester.StartDate)
            {
                semesterStatus = 1;
            }
            if (currentTime >= existedSemester.StartDate && currentTime <= existedSemester.EndDate)
            {
                semesterStatus = 2;
            }
            if (currentTime > existedSemester.EndDate)
            {
                semesterStatus = 3;
            }
            existedSemester.SemesterStatus = semesterStatus;

            _unitOfWork.SemesterRepository.Update(existedSemester);
            var result = await _unitOfWork.SaveChangesAsync();
            if (result)
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = true,
                    Title = "Update Semester successfully",
                    Result = existedSemester
                };
            }
            else
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = false,
                    Title = "Update Semester failed"
                };
            }
        }

        public async Task<Semester?> GetById(int id)
        {
            var includes = new Expression<Func<Semester, object?>>[]
            {
                s => s.Classes
            };
            return await _unitOfWork.SemesterRepository
                .Get(s => s.SemesterID == id, includes)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<ServiceResponseVM<IEnumerable<Semester>>> GetAll(int startPage, int endPage, int quantity, string? semesterCode, int? semesterStatus, DateTime? startDate, DateTime? endDate)
        {
            var result = new ServiceResponseVM<IEnumerable<Semester>>()
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
            ParameterExpression pe = Expression.Parameter(typeof(Semester), "s");
            MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Semester.IsDeleted)), Expression.Constant(false)));

            if (containsMethod is null)
            {
                errors.Add("Method Contains can not found from string type");
                return result;
            }

            if(semesterCode is not null)
            {
                expressions.Add(Expression.Call(Expression.Property(pe, nameof(Semester.SemesterCode)), containsMethod, Expression.Constant(semesterCode)));
            }

            if(semesterStatus is not null)
            {
                expressions.Add(Expression.Call(Expression.Property(pe, nameof(Semester.SemesterStatus)), containsMethod, Expression.Constant(semesterStatus)));
            }

            if(startDate is not null)
            {
                var startDateValue = startDate.Value;
                var startDateOnly = new DateOnly(startDateValue.Year, startDateValue.Month, startDateValue.Day);
                expressions.Add(Expression.LessThanOrEqual(Expression.Property(pe, nameof(Semester.StartDate)), Expression.Constant(startDateOnly)));
            }

            if (endDate is not null)
            {
                var endDateValue = endDate.Value;
                var endDateOnly = new DateOnly(endDateValue.Year, endDateValue.Month, endDateValue.Day);
                expressions.Add(Expression.GreaterThanOrEqual(Expression.Property(pe, nameof(Semester.EndDate)), Expression.Constant(endDateOnly)));
            }

            Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
            Expression<Func<Semester, bool>> where = Expression.Lambda<Func<Semester, bool>>(combined, pe);

            var semesters = await _unitOfWork.SemesterRepository
                .Get(where)
                .AsNoTracking()
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult)
                .ToListAsync();

            result.IsSuccess = true;
            result.Result = semesters;
            result.Title = "Get successfully";

            return result;
        }
    }

}

        
    
