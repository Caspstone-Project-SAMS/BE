﻿using Base.IService.IService;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
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
        private readonly ICurrentUserService _currentUserService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public StudentService(IUnitOfWork unitOfWork, IValidateGet validateGet, UserManager<User> userManager, IMailService mailService, ICurrentUserService currentUserService, IServiceScopeFactory serviceScopeFactory)
        {
            _unitOfWork = unitOfWork;
            _validateGet = validateGet;
            _userManager = userManager;
            _mailService = mailService;
            _currentUserService = currentUserService;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task<ServiceResponseVM<List<StudentVM>>> CreateStudent(List<StudentVM> newEntities)
        {
            ConcurrentBag<StudentVM> createdStudents = new ConcurrentBag<StudentVM>();
            ConcurrentBag<string> errors = new ConcurrentBag<string>();
            SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

            DbContextFactory dbFactory = new DbContextFactory();
            using IServiceScope serviceScope = _serviceScopeFactory.CreateScope();

            ParallelOptions parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.25) * 2.0))
            };
            await Parallel.ForEachAsync(newEntities, parallelOptions, async (newEntity, state) =>
            {
                var dbContext = dbFactory.CreateDbContext(Array.Empty<string>());

                var newUserManager = CreateUserManagerFromExisting(_userManager, dbContext, serviceScope.ServiceProvider);

                var existedStudent = await dbContext.Set<Student>()
                    .Where(st => st.StudentCode.Equals(newEntity.StudentCode))
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                if (existedStudent is not null)
                {
                    errors.Add($"StudentCode {newEntity.StudentCode} is already taken");
                    return;
                }

                var existingUserEmail = await dbContext.Users
                    .Where(u => u.Email == newEntity.Email)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
                if (existingUserEmail != null)
                {
                    errors.Add($"User with email {newEntity.Email} already exists.");
                    return;
                }

                Student newStudent = new Student
                {
                    StudentCode = newEntity.StudentCode.ToUpper(),
                    CreatedBy = _currentUserService.UserId,
                    CreatedAt = ServerDateTime.GetVnDateTime(),
                };

                try
                {
                    await dbContext.Students.AddAsync(newStudent);

                    var result = (await dbContext.SaveChangesAsync()) > 0;

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
                            CreatedBy = _currentUserService.UserId,
                            CreatedAt = ServerDateTime.GetVnDateTime(),
                        };
                        var password = GenerateRandomPassword(6);

                        var identityResult = await newUserManager.CreateAsync(newUser, password);

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
                            _ = _mailService.SendMailAsync(emailMessage);
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

            });

            var createdStudentsList = createdStudents.ToList();
            return new ServiceResponseVM<List<StudentVM>>
            {
                IsSuccess = createdStudentsList.Count() > 0,
                Title = createdStudentsList.Count() > 0 ? "Create Students Result" : "Create Students failed",
                Result = createdStudentsList,
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

        public async Task<IEnumerable<Student>> GetStudentsByClassID(int classID, int startPage, int endPage, int? quantity, Guid? userId)
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
                s => s.User!.StudentClasses            
            };

            var result = _unitOfWork.StudentRepository
            .Get(s => s.User != null && s.User.StudentClasses
                .Any(c => c.ClassID == classID), includes: includes)
            .Where(c => c.IsDeleted == false)
            //.AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult);

            if (userId is not null)
            {
                result = result.Where(s => s.User != null && s.User.Id == userId);
            }

            return await result.ToArrayAsync();
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

        public async Task<ServiceResponseVM<List<StudentClassVM>>> AddStudentToClass(List<StudentClassVM> newEntities,int semesterId)
        {
            List<StudentClassVM> responseList = new List<StudentClassVM>();
            List<string> errors = new List<string>();
            var existedSemester = await _unitOfWork.SemesterRepository.Get(s => s.SemesterID == semesterId && !s.IsDeleted).SingleOrDefaultAsync();
            if (existedSemester is null)
            {
                return new ServiceResponseVM<List<StudentClassVM>>
                {
                    IsSuccess = false,
                    Title = "Add new student to class failed",
                    Errors = new string[1] { "Semester not Existed" }
                };
            }
            foreach(var newEntity in newEntities)
            {
                    
                    var existedStudent = await _unitOfWork.StudentRepository.Get(s => s.StudentCode.Equals(newEntity.StudentCode) && !s.IsDeleted, includes: u => u.User).FirstOrDefaultAsync();
                    if (existedStudent is null)
                    {
                        errors.Add($"Student with code {newEntity.StudentCode} not existed");
                        continue;
                    }

                    var existedClass = await _unitOfWork.ClassRepository.Get(c => c.ClassCode.Equals(newEntity.ClassCode) && c.SemesterID == semesterId && !c.IsDeleted).FirstOrDefaultAsync();
                    if (existedClass is null)
                    {
                        errors.Add($"Class with code {newEntity.ClassCode} not existed in Semester {existedSemester.SemesterCode}");
                        continue;
                    }

                    var check = await _unitOfWork.StudentClassRepository.Get("StudentClass",s => s.StudentID.Equals(existedStudent.User!.Id) && s.ClassID == existedClass.ClassID).FirstOrDefaultAsync();
                    if (check is not null)
                    {
                        errors.Add($"Student with code {newEntity.StudentCode} already existed in Class {newEntity.ClassCode}");
                        continue;
                    }

                    StudentClass newStudentClass = new StudentClass()
                    {
                        StudentID = existedStudent.User!.Id,
                        ClassID = existedClass.ClassID,
                        AbsencePercentage = 0,
                        CreatedBy = existedClass.LecturerID.ToString(),
                        CreatedAt = ServerDateTime.GetVnDateTime(),
                        IsDeleted = false
                    };
                    
                    await _unitOfWork.StudentClassRepository.AddAsync(newStudentClass);
                    responseList.Add(newEntity);
              
            }//


                    if(errors.Count > 0)
                    {
                        return new ServiceResponseVM<List<StudentClassVM>>
                            {
                                 IsSuccess = false,
                                 Title = "Add new student to class failed",
                                 Errors = errors.Distinct().ToArray()
                            };
                    }


                    try
                    {
                        var result = await _unitOfWork.SaveChangesAsync();

                        if (result)
                        {
                            return new ServiceResponseVM<List<StudentClassVM>>
                            {
                                IsSuccess = true,
                                Title = "Add new student to class successfully",
                                Result = responseList,
                            };
                        }
                        else
                        {
                            return new ServiceResponseVM<List<StudentClassVM>>
                            {
                                IsSuccess = false,
                                Title = "Add new student to class failed",
                                Errors = errors.Distinct().ToArray()
                            };
                        }
                
                    }
                    catch (DbUpdateException ex)
                    {
                        errors.Add($"DbUpdateException: {ex.Message}");
                        return new ServiceResponseVM<List<StudentClassVM>>
                        {
                            IsSuccess = false,
                            Title = "Add new student to class failed",
                            Errors = errors.Distinct().ToArray()
                        };

                    }
                    catch (OperationCanceledException ex)
                    {
                        errors.Add($"OperationCanceledException: {ex.Message}");
                        return new ServiceResponseVM<List<StudentClassVM>>
                        {
                            IsSuccess = false,
                            Title = "Add new student to class failed",
                            Errors = errors.Distinct().ToArray()
                        };

                }
        }

        public async Task<User?> GetById(Guid id)
        {
            var includes = new Expression<Func<User, object?>>[]
            {
                u => u.Student,
                u => u.Student!.FingerprintTemplates,
                u => u.EnrolledClasses,
            };
            return await _unitOfWork.UserRepository
                .Get(s => s.StudentID == id, includes)
                .Include(nameof(User.EnrolledClasses) + "." + nameof(Class.StudentClasses))
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<Student>> GetStudentsByClassIdv2(int startPage, int endPage, int quantity, Guid? userId, int? classID)
        {
            int quantityResult = 0;
            _validateGet.ValidateGetRequest(ref startPage, ref endPage, quantity, ref quantityResult);
            if (quantityResult == 0)
            {
                quantityResult = 1;
            }

            var result = await _unitOfWork.StudentRepository
                .Get(s => !s.IsDeleted)
                .Include(s => s.FingerprintTemplates)
                .Include(s => s.User!.StudentClasses)
                .ToListAsync();

            if(userId is not null)
            {
                result = result.Where(s => s.User != null && s.User.Id == userId).ToList();
            }

            if(classID is not null)
            {
                result = result.Where(s => s.User != null && s.User.StudentClasses.Any(c => c.ClassID == classID)).ToList();
            }

            return result
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult);
        }

        private UserManager<User> CreateUserManagerFromExisting(UserManager<User> existingUserManager, ApplicationDbContext newDbContext, IServiceProvider services)
        {
            // Create a new UserStore using the new DbContext
            var newUserStore = new UserStore<User, IdentityRole<Guid>, ApplicationDbContext, Guid>(newDbContext);

            // Retrieve dependencies from the existing UserManager
            var options = Options.Create(existingUserManager.Options);
            var passwordHasher = existingUserManager.PasswordHasher;
            var userValidators = existingUserManager.UserValidators;
            var passwordValidators = existingUserManager.PasswordValidators;
            var keyNormalizer = existingUserManager.KeyNormalizer;
            var errorDescriber = existingUserManager.ErrorDescriber;

            var logger = existingUserManager.Logger as ILogger<UserManager<User>>;
            if (logger == null)
            {
                // Create a logger if the existing one is not of the correct type
                var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole()); // Customize the logger factory as needed
                logger = loggerFactory.CreateLogger<UserManager<User>>();
            }

            // Create a new instance of UserManager using the new UserStore and existing dependencies
            var newUserManager = new UserManager<User>(
                newUserStore,
                options,
                passwordHasher,
                userValidators,
                passwordValidators,
                keyNormalizer,
                errorDescriber,
                services,
                logger
            );

            return newUserManager;
        }
    }
}
