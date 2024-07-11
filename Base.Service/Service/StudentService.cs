using Base.IService.IService;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.Common;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.AspNetCore.Identity;
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
        private readonly UserManager<User> _userManager;
        private readonly IMailService _mailService;
        public StudentService(IUnitOfWork unitOfWork, IValidateGet validateGet, UserManager<User> userManager, IMailService mailService)
        {
            _unitOfWork = unitOfWork;
            _validateGet = validateGet;
            _userManager = userManager;
            _mailService = mailService;
        }
        public async Task<ServiceResponseVM<List<StudentVM>>> CreateStudent(List<StudentVM> newEntities)
        {
            List<StudentVM> createdStudents = new List<StudentVM>();
            List<string> errors = new List<string>();

            foreach (var newEntity in newEntities)
            {
                var existedStudent = await _unitOfWork.StudentRepository.Get(st => st.StudentCode.Equals(newEntity.StudentCode)).SingleOrDefaultAsync();
                if (existedStudent is not null)
                {
                    errors.Add($"StudentCode {newEntity.StudentCode} is already taken");
                    continue;
                }

                var existingUserEmail = await _unitOfWork.UserRepository.Get(u => u.Email == newEntity.Email).SingleOrDefaultAsync();
                if (existingUserEmail != null)
                {
                    errors.Add($"User with email {newEntity.Email} already exists.");
                    continue;
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
                        var studentId = newStudent.StudentID;

                        User newUser = new User
                        {
                            UserName = newEntity.StudentCode,
                            StudentID = studentId,
                            Email = newEntity.Email,
                            DisplayName = newEntity.DisplayName,
                            RoleID = 3,
                            CreatedBy = newEntity.CreateBy!,
                            CreatedAt = ServerDateTime.GetVnDateTime(),
                        };
                        var password = GenerateRandomPassword(6);
                        var identityResult = await _userManager.CreateAsync(newUser, password);

                        if (identityResult.Succeeded)
                        {
                            createdStudents.Add(newEntity);
                            var emailMessage = new Message
                            {
                                To = newEntity.Email,
                                Subject = "Your account has been created",
                                Content = $@"<html>
                                <body>
                                <p>Dear Student,</p>
                                <p>Your account has been created successfully. Here are your login details:</p>
                                <ul>
                                     <li><strong>Username:</strong> {newEntity.StudentCode}</li>
                                     <li><strong>Password:</strong> {password}</li>
                                </ul>
                                <p>Best regards,<br>SAMS Team</p>
                                </body>
                                </html>"
                            };
                            await _mailService.SendMailAsync(emailMessage);
                        }
                        else
                        {
                            errors.Add($"Failed to create User for StudentCode {newEntity.StudentCode}");
                        }
                    }
                    else
                    {
                        errors.Add($"Failed to save Student with StudentCode {newEntity.StudentCode}");
                    }
                }
                catch (DbUpdateException ex)
                {
                    errors.Add($"DbUpdateException for StudentCode {newEntity.StudentCode}: {ex.Message}");
                }
                catch (OperationCanceledException ex)
                {
                    errors.Add($"OperationCanceledException for StudentCode {newEntity.StudentCode}: {ex.Message}");
                }
            }

            return new ServiceResponseVM<List<StudentVM>>
            {
                IsSuccess = createdStudents.Count > 0,
                Title = createdStudents.Count > 0 ? "Create Students Result" : "Create Students failed",
                Result = createdStudents,
                Errors = errors.ToArray()
            };
        }


        private string GenerateRandomPassword(int length)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder result = new StringBuilder();
            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                result.Append(validChars[random.Next(validChars.Length)]);
            }
            return result.ToString();
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
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Student.IsDeleted)), Expression.Constant(false)));
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

        public async Task<IEnumerable<Student>> GetStudentsByClassID(int classID, int startPage, int endPage, int? quantity)
        {
            int quantityResult = 0;
            _validateGet.ValidateGetRequest(ref startPage, ref endPage, quantity, ref quantityResult);
            if (quantityResult == 0)
            {
                throw new ArgumentException("Error when get quantity per page");
            }
            var includes = new Expression<Func<Student, object?>>[]
            {
                s => s.FingerprintTemplates,
                s => s.User,
                s => s.User!.EnrolledClasses,
                s => s.User!.StudentClasses
                
            };

            return await _unitOfWork.StudentRepository
            .Get(s => s.User != null && s.User.EnrolledClasses
            .Any(c => c.ClassID == classID), includes: includes)
            .Where(c => c.IsDeleted == false)
            .AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult)
            .ToArrayAsync();
        }

        public async Task<ServiceResponseVM> Delete(Guid id)
        {
            var existedStudent = await _unitOfWork.StudentRepository.Get(st => st.StudentID == id && !st.IsDeleted).FirstOrDefaultAsync();
            if (existedStudent is null)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete student failed",
                    Errors = new string[1] { "Student not found" }
                };
            }

            existedStudent.IsDeleted = true;
            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = true,
                        Title = "Delete student successfully"
                    };
                }
                else
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Title = "Delete student failed",
                        Errors = new string[1] { "Save changes failed" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete student failed",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete student failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }

        public async Task<ServiceResponseVM<List<StudentClassVM>>> AddStudentToClass(List<StudentClassVM> newEntities)
        {
            List<StudentClassVM> responseList = new List<StudentClassVM>();
            List<string> errors = new List<string>();
            foreach(var newEntity in newEntities)
            {
                try
                {
                    var existedStudent = await _unitOfWork.StudentRepository.Get(s => s.StudentCode.Equals(newEntity.StudentCode), includes: u => u.User).SingleOrDefaultAsync();
                    if (existedStudent is null)
                    {
                        errors.Add($"Student with code {newEntity.StudentCode} not existed");
                        continue;
                    }

                    var existedClass = await _unitOfWork.ClassRepository.Get(c => c.ClassCode.Equals(newEntity.ClassCode)).SingleOrDefaultAsync();
                    if (existedClass is null)
                    {
                        errors.Add($"Class with code {newEntity.ClassCode} not existed");
                        continue;
                    }

                    StudentClass newStudentClass = new StudentClass()
                    {
                        StudentID = existedStudent.User!.Id,
                        ClassID = existedClass.ClassID,
                        AbsencePercentage = 0,
                        CreatedBy = "",
                        CreatedAt = ServerDateTime.GetVnDateTime(),
                        IsDeleted = false
                    };
                    
                    await _unitOfWork.StudentClassRepository.AddAsync(newStudentClass);
                    responseList.Add(newEntity);
                }
                catch (DbUpdateException ex)
                {
                    errors.Add($"DbUpdateException for ClassCode {newEntity.ClassCode} and StudentCode {newEntity.StudentCode}: {ex.Message}");
                    continue;
                }
                catch (OperationCanceledException ex)
                {
                    errors.Add($"OperationCanceledException for ClassCode {newEntity.ClassCode} and StudentCode {newEntity.StudentCode}: {ex.Message}");
                    continue;
                }
            }
            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                return new ServiceResponseVM<List<StudentClassVM>>
                {
                    IsSuccess = errors.Count == 0,
                    Title = errors.Count == 0 ? "Add new student to class successfully" : "Partial success in adding new students to class",
                    Result = responseList,
                    Errors = errors.ToArray()
                };
            }
            else
            {
                return new ServiceResponseVM<List<StudentClassVM>>
                {
                    IsSuccess = false,
                    Title = "Add new student to class failed",
                    Errors = errors.ToArray()
                };
            }

        }
    }
}
