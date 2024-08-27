using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidateGet _validateGet;
        private readonly IServiceProvider _serviceProvider;
        public ClassService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IValidateGet validateGet, IServiceProvider serviceProvider)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _validateGet = validateGet;
            _serviceProvider = serviceProvider;
        }

        public async Task<ServiceResponseVM<Class>> Create(ClassVM newEntity)
        {
            var errors = new List<string>();

            var existedSemester = await _unitOfWork.SemesterRepository
                .Get(r => !r.IsDeleted && r.SemesterID == newEntity.SemesterId)
                .FirstOrDefaultAsync();
            if (existedSemester is null)
            {
                errors.Add("Semester not found");
            }

            var existedRoom = await _unitOfWork.RoomRepository
                .Get(r => !r.IsDeleted && r.RoomID == newEntity.RoomId)
                .FirstOrDefaultAsync();
            if (existedRoom is null)
            {
                errors.Add("Room not found");
            }

            var existedClassCode = await _unitOfWork.ClassRepository
                .Get(r => !r.IsDeleted && (r.ClassCode.Equals(newEntity.ClassCode) && r.SemesterID == newEntity.SemesterId ))
                .FirstOrDefaultAsync();
            if (existedClassCode is not null)
            {
                errors.Add("Class Code is already existed");
            }

            var existedSubject = await _unitOfWork.SubjectRepository
                .Get(r => !r.IsDeleted && r.SubjectID == newEntity.SubjectId)
                .FirstOrDefaultAsync();
            if (existedSubject is null)
            {
                errors.Add("Subject not found");
            }

            var existedLecturer = await _unitOfWork.UserRepository
                .Get(u => !u.Deleted && u.Id == newEntity.LecturerID && u.Role!.Name == "Lecturer")
                .FirstOrDefaultAsync();
            if(existedLecturer is null)
            {
                errors.Add("Lecturer not found");
            }

            var existedSlotType = _unitOfWork.SlotTypeRepository
                .Get(s => !s.IsDeleted && s.SlotTypeID == newEntity.SlotTypeId)
                .FirstOrDefault();
            if(existedSlotType is null)
            {
                errors.Add("Slot type not found");
            }

            if(errors.Count() > 0)
            {
                return new ServiceResponseVM<Class>
                {
                    IsSuccess = false,
                    Title = "Create Class failed",
                    Errors = errors
                };
            }

            Class newClass = new Class()
            {
                ClassCode = newEntity.ClassCode,
                ClassStatus = 1,
                SemesterID = newEntity.SemesterId,
                RoomID = newEntity.RoomId,
                SubjectID = newEntity.SubjectId,
                LecturerID = newEntity.LecturerID,
                SlotTypeId = newEntity.SlotTypeId,
                CreatedBy = _currentUserService.UserId,
                CreatedAt = ServerDateTime.GetVnDateTime(),
                IsDeleted = false
            };

            try
            {
                await _unitOfWork.ClassRepository.AddAsync(newClass);

                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponseVM<Class>
                    {
                        IsSuccess = true,
                        Title = "Create Class successfully",
                        Result = newClass
                    };
                }
                else
                {
                    return new ServiceResponseVM<Class>
                    {
                        IsSuccess = false,
                        Title = "Create Class failed",
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM<Class>
                {
                    IsSuccess = false,
                    Title = "Create Class failed",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM<Class>
                {
                    IsSuccess = false,
                    Title = "Create Class failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }

        public async Task<Class?> GetById(int classId)
        {
            DbContextFactory dbFactory = new DbContextFactory();

            var studentIncludes = new Expression<Func<User, object?>>[]
            {
                s => s.StudentClasses.Where(sc => sc.ClassID == classId),
                s => s.Student
            };
            
            var classIncludes = new Expression<Func<Class, object?>>[]
            {
                c => c.Semester,
                c => c.Room,
                c => c.Subject,
                c => c.Lecturer!.Employee,
                c => c.Schedules,
            };

            var dbContext1 = dbFactory.CreateDbContext(Array.Empty<string>());
            var dbContext2 = dbFactory.CreateDbContext(Array.Empty<string>());
            var dbContext3 = dbFactory.CreateDbContext(Array.Empty<string>());

            var students = dbContext1
                .Set<User>()
                .Where(u => !u.Deleted && u.StudentClasses.Any(c => c.ClassID == classId))
                .Include(s => s.StudentClasses.Where(sc => sc.ClassID == classId))
                .Include(s => s.Student)
                .AsNoTracking()
                .ToListAsync();

            var schedules = dbContext3
                .Set<Schedule>()
                .Where(s => !s.IsDeleted && s.ClassID == classId)
                .Include(s => s.Slot)
                .Include(s => s.Room)
                .AsNoTracking()
                .ToListAsync();

            var existedClass = dbContext2
                .Set<Class>()
                .Where(c => !c.IsDeleted && c.ClassID == classId)
                .Include(c => c.Semester)
                .Include(c => c.Room)
                .Include(c => c.Subject)
                .Include(c => c.Lecturer!.Employee)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            await Task.WhenAll(students, schedules, existedClass);

            var result = existedClass.Result;
            if(result != null)
            {
                result.Students = students.Result;
                result.Schedules = schedules.Result;
            }

            dbContext1.Dispose();
            dbContext2.Dispose();
            dbContext3.Dispose();

            return result;
        }

        public async Task<ServiceResponseVM<IEnumerable<Class>>> GetAllClasses(int startPage, int endPage, int quantity, int? semesterId, string? classCode, int? classStatus, int? roomID, int? subjectID, Guid? lecturerId, Guid? studentId, int? scheduleId)
        {
            var result = new ServiceResponseVM<IEnumerable<Class>>()
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
            ParameterExpression pe = Expression.Parameter(typeof(Class), "c");
            MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            MethodInfo? anyMethodStudent = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(StudentClass));

            MethodInfo? anyMethodSchedule = typeof(Enumerable).GetMethods()
                .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                .MakeGenericMethod(typeof(Schedule));

            if (containsMethod is null)
            {
                errors.Add("Method Contains can not found from string type");
                return result;
            }
            if(anyMethodStudent is null || anyMethodSchedule is null)
            {
                errors.Add("Method Any can not found from Enumerable type");
                return result;
            }

            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.IsDeleted)), Expression.Constant(false)));

            if(semesterId is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.SemesterID)), Expression.Constant(semesterId)));
            }

            if(classCode is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.ClassCode)), Expression.Constant(classCode)));
            }

            if(classStatus is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.ClassStatus)), Expression.Constant(classStatus)));
            }

            if(roomID is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.RoomID)), Expression.Constant(roomID)));
            }

            if(subjectID is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.SubjectID)), Expression.Constant(subjectID)));
            }

            if(lecturerId is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.LecturerID)), Expression.Constant(lecturerId)));
            }

            if(studentId is not null)
            {
                var studentClassParameter = Expression.Parameter(typeof(StudentClass), "a");
                var studentIdProperty = Expression.Property(studentClassParameter, "StudentID");
                var studentIdCondition = Expression.Equal(studentIdProperty, Expression.Constant(studentId));
                var lambda = Expression.Lambda(studentIdCondition, studentClassParameter);
                var expression = Expression.Call(anyMethodStudent, Expression.Property(pe, nameof(Class.StudentClasses)), lambda);
                expressions.Add(expression);
            }

            if (scheduleId is not null)
            {
                var scheduleParameter = Expression.Parameter(typeof(Schedule), "s");
                var scheduleIdProperty = Expression.Property(scheduleParameter, "ScheduleID");
                var scheduleIdCondition = Expression.Equal(scheduleIdProperty, Expression.Constant(scheduleId));
                var lambda = Expression.Lambda(scheduleIdCondition, scheduleParameter);
                expressions.Add(Expression.Call(anyMethodSchedule, Expression.Property(pe, nameof(Class.Schedules)), lambda));
            }

            Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
            Expression<Func<Class, bool>> where = Expression.Lambda<Func<Class, bool>>(combined, pe);

            var includes = new Expression<Func<Class, object?>>[]
            {
                c => c.Lecturer,
                c => c.Semester,
                c => c.Subject,
                c => c.Room
            };

            var classes = await _unitOfWork.ClassRepository
                .Get(where, includes)
                .AsNoTracking()
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult)
                .ToArrayAsync();

            result.IsSuccess = true;
            result.Result = classes;
            result.Title = "Get successfully";

            return result;
        }

        public async Task<IEnumerable<string>> GetAllClassCodes(int? semesterId, Guid? userId)
        {
            var expressions = new List<Expression>();
            ParameterExpression pe = Expression.Parameter(typeof(Class), "c");

            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.IsDeleted)), Expression.Constant(false)));

            if (semesterId is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.SemesterID)), Expression.Constant(semesterId)));
            }

            if (userId is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.LecturerID)), Expression.Constant(userId)));
            }

            Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
            Expression<Func<Class, bool>> where = Expression.Lambda<Func<Class, bool>>(combined, pe);

            return await _unitOfWork.ClassRepository
                .Get(where)
                .Select(c => c.ClassCode.ToUpper())
                .ToArrayAsync();
        }

        public async Task<ServiceResponseVM<Class>> UpdateClass(int classId, UpdateClassVM resource)
        {
            var result = new ServiceResponseVM<Class>
            {
                IsSuccess = false
            };
            var errors = new List<string>();

            var existedClass = _unitOfWork.ClassRepository
                .Get(c => !c.IsDeleted && c.ClassID == classId)
                .FirstOrDefault();

            if(existedClass is null)
            {
                result.IsSuccess = false;
                result.Errors = new string[1] { "Class not found" };
                return result;
            }

            if (resource.SemesterId is null &&
            resource.RoomId is null &&
            resource.SubjectId is null &&
            resource.LecturerID is null &&
            resource.ClassCode is null)
            {
                result.IsSuccess = true;
                result.Title = "Update class successfully";
                result.Result = existedClass;
                return result;
            }

            var copyClass = (Class)existedClass.Clone();

            if(resource.SemesterId is not null)
            {
                var checkExistedSemester = _unitOfWork.SemesterRepository
                    .Get(s => !s.IsDeleted && s.SemesterID == resource.SemesterId)
                    .AsNoTracking()
                    .FirstOrDefault() is null;
                if (checkExistedSemester)
                {
                    errors.Add("Semester not found");
                }
                else
                {
                    existedClass.SemesterID = (int)resource.SemesterId;
                }
            }

            if(resource.RoomId is not null)
            {
                var checkExistedRoom = _unitOfWork.RoomRepository
                    .Get(r => !r.IsDeleted && r.RoomID == resource.RoomId)
                    .AsNoTracking()
                    .FirstOrDefault() is null;
                if (checkExistedRoom)
                {
                    errors.Add("Room not found");
                }
                else
                {
                    existedClass.RoomID = (int)resource.RoomId;
                }
            }

            if(resource.SubjectId is not null)
            {
                var checkExistedSubject = _unitOfWork.SubjectRepository
                    .Get(s => !s.IsDeleted && s.SubjectID == resource.SubjectId)
                    .AsNoTracking()
                    .FirstOrDefault() is null;
                if (checkExistedSubject)
                {
                    errors.Add("Subject not found");
                }
                else
                {
                    existedClass.SubjectID = (int)resource.SubjectId;
                }
            }

            if(resource.LecturerID is not null)
            {
                var checkExistedLecturer = _unitOfWork.UserRepository
                    .Get(u => !u.Deleted && u.Id == resource.LecturerID)
                    .AsNoTracking()
                    .FirstOrDefault() is null;
                if (checkExistedLecturer)
                {
                    errors.Add("Lecturer not found");
                }
                else
                {
                    existedClass.LecturerID = (Guid)resource.LecturerID;
                }
            }

            if(resource.ClassCode is not null)
            {
                var belongedSemesterId = resource.SemesterId is null ? existedClass.SemesterID : resource.SemesterId;
                var checkExistedClassCode = _unitOfWork.ClassRepository
                    .Get(c => !c.IsDeleted && c.ClassID != existedClass.ClassID && c.SemesterID == belongedSemesterId && c.ClassCode == resource.ClassCode.Trim())
                    .AsNoTracking()
                    .FirstOrDefault() is not null;
                if (checkExistedClassCode)
                {
                    errors.Add("Class code is already taken");
                }
                else
                {
                    existedClass.ClassCode = resource.ClassCode;
                }
            }

            if(errors.Count() > 0)
            {
                result.IsSuccess = false;
                result.Errors = errors;
                return result;
            }

            if (TwoObjectsAreTheSame(copyClass, existedClass))
            {
                result.IsSuccess = true;
                result.Title = "Update class successfully";
                result.Result = existedClass;
                return result;
            }

            try
            {
                var finalResult = await _unitOfWork.SaveChangesAsync();
                if (finalResult)
                {
                    result.IsSuccess = true;
                    result.Result = existedClass;
                    result.Title = "Update class successfully";
                    return result;
                }

                result.IsSuccess = false;
                result.Errors = new string[1] { "Error when saving changes" };
                return result;
            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Errors = new string[2] { "Error when saving changes", ex.Message };
                return result;
            }
        }
        
        public async Task<ServiceResponseVM> DeleteClassById(int classId)
        {
            var existedClass = _unitOfWork.ClassRepository
                .Get(c => !c.IsDeleted && c.ClassID == classId,
                new Expression<Func<Class, object?>>[]
                {
                    c => c.Schedules.Where(s => !s.IsDeleted)
                })
                .FirstOrDefault();

            if(existedClass is null)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete class failed",
                    Errors = new string[1] { "Class not found" }
                };
            }

            if(existedClass.Schedules.Count() > 0)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete class failed",
                    Errors = new string[1] { "Delete all schedules of the class first" }
                };
            }

            existedClass.IsDeleted = true;

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = true,
                        Title = "Delete class successfully"
                    };
                }

                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete class failed",
                    Errors = new string[1] { "Error when saving changes" }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete class failed",
                    Errors = new string[2] { "Error when saving changes", ex.Message }
                };
            }
        }

        private bool TwoObjectsAreTheSame(Class object1, Class object2)
        {
            return (object1.SemesterID == object2.SemesterID && object1.RoomID == object2.RoomID &&
                object1.SubjectID == object2.SubjectID && object1.LecturerID == object2.LecturerID &&
                object1.ClassCode == object2.ClassCode);
        }
    }
}
