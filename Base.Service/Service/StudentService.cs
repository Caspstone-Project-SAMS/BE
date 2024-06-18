using Base.IService.IService;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.Common;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
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
    public class StudentService : IStudentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidateGet _validateGet;
        public StudentService(IUnitOfWork unitOfWork, IValidateGet validateGet)
        {
            _unitOfWork = unitOfWork;
            _validateGet = validateGet;
        }
        public async Task<ServiceResponseVM<Student>> CreateStudent(StudentVM newEntity)
        {
            var existedStudent = await _unitOfWork.StudentRepository.Get(st => st.StudentCode.Equals(newEntity.StudentCode)).SingleOrDefaultAsync();
            if (existedStudent is not null)
            {
                return new ServiceResponseVM<Student>
                {
                    IsSuccess = false,
                    Title = "Create Student failed",
                    Errors = new string[1] { "StudentCode is already taken" }
                };

            }

            Student newStudent = new Student
            {
                StudentCode = newEntity.StudentCode,
                CreatedBy = newEntity.CreateBy!,
                CreatedAt = ServerDateTime.GetVnDateTime(),
            };

            try
            {
                await _unitOfWork.StudentRepository.AddAsync(newStudent);

                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponseVM<Student>
                    {
                        IsSuccess = true,
                        Title = "Create Student successfully",
                        Result = newStudent
                    };
                }
                else
                {
                    return new ServiceResponseVM<Student>
                    {
                        IsSuccess = false,
                        Title = "Create Student failed",
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM<Student>
                {
                    IsSuccess = false,
                    Title = "Create student failed",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM<Student>
                {
                    IsSuccess = false,
                    Title = "Create student failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }

        public async Task<IEnumerable<Student>> GetStudents(int startPage, int endPage, int? quantity, Guid? studentID, string? studentCode)
        {
            int quantityResult = 0;
            _validateGet.ValidateGetRequest(ref startPage, ref endPage, quantity, ref quantityResult);
            if (quantityResult == 0)
            {
                throw new ArgumentException("Error when get quantity per page");
            }
            var expressions = new List<Expression>();
            ParameterExpression pe = Expression.Parameter(typeof(Student), "s");
            MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            if (containsMethod is null)
            {
                throw new ArgumentNullException("Method Contains can not found from string type");
            }

            if (studentCode is not null)
            {
                expressions.Add(Expression.Call(Expression.Property(pe, nameof(Student.StudentCode)), containsMethod, Expression.Constant(studentCode)));
            }

            if (studentID is not null)
            {
                expressions.Add(Expression.Call(Expression.Property(pe, nameof(Student.StudentID)), containsMethod, Expression.Constant(studentID)));
            }
           
            Expression combined = null!;

            if (expressions.Count > 0)
            {
                combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
            }
            else
            {
                combined = Expression.Constant(true);
            }
            Expression<Func<Student, bool>> where = Expression.Lambda<Func<Student, bool>>(combined, pe);
            var includes = new Expression<Func<Student, object?>>[]
            {
                s => s.FingerprintTemplates,
                s => s.User,
            };
            return await _unitOfWork.StudentRepository
            .Get(where,includes: includes)
            .AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult)
            .ToArrayAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByClassID(int classID)
        {
            var includes = new Expression<Func<Student, object?>>[]
            {
                s => s.FingerprintTemplates,
                s => s.User,
                s => s.User!.EnrolledClasses
            };

            return await _unitOfWork.StudentRepository
            .Get(s => s.User != null && s.User.EnrolledClasses.Any(c => c.ClassID == classID), includes: includes)
    .ToArrayAsync();
        }
    }
}
