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
            var existedSemesterCode = await _unitOfWork.SemesterRepository.Get(s => s.SemesterCode == newEntity.SemesterCode).SingleOrDefaultAsync();
            if (existedSemesterCode is not null)
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = false,
                    Title = "Create Semester failed",
                    Errors = new string[1] { "Semester Code is already taken" }
                };
            }

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

            Semester newSemester = new Semester
            {
                SemesterCode = newEntity.SemesterCode,
                SemesterStatus = newEntity.SemesterStatus,
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
            var existedSemester = await _unitOfWork.SemesterRepository.Get(r => r.SemesterID == id && !r.IsDeleted).FirstOrDefaultAsync();
            if (existedSemester is null)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete semester failed",
                    Errors = new string[1] { "Semester not found" }
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
            var existedSemester = await _unitOfWork.SemesterRepository.Get(s => s.SemesterID == id && !s.IsDeleted).SingleOrDefaultAsync();
            if (existedSemester is null)
            {
                return new ServiceResponseVM<Semester>
                {
                    IsSuccess = false,
                    Title = "Update Semester failed",
                    Errors = new string[1] { "Semester not found" }
                };
            }

            if (updateSemester.SemesterCode != existedSemester.SemesterCode)
            {
                var checkSemesterCode = _unitOfWork.SemesterRepository.Get(s => s.SemesterCode == updateSemester.SemesterCode && !s.IsDeleted).FirstOrDefault() is not null;
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
                var overlappingSemester = await _unitOfWork.SemesterRepository.Get(s =>
                !s.IsDeleted &&
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



            existedSemester.SemesterStatus = updateSemester.SemesterStatus;
            existedSemester.SemesterCode = updateSemester.SemesterCode;
            existedSemester.StartDate = updateSemester.StartDate;
            existedSemester.EndDate = updateSemester.EndDate;
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

        
    
