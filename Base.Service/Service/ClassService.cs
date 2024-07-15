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
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidateGet _validateGet;
        public ClassService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService, IValidateGet validateGet)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
            _validateGet = validateGet;
        }

        public async Task<ServiceResponseVM<Class>> Create(ClassVM newEntity)
        {
            var existedRoom = await _unitOfWork.RoomRepository.Get(r => r.RoomName.Equals(newEntity.RoomName)).FirstOrDefaultAsync();
            if (existedRoom is null)
            {
                return new ServiceResponseVM<Class>
                {
                    IsSuccess = false,
                    Title = "Create Class failed",
                    Errors = new string[1] { "Room Name is not exist" }
                };

            }
            var existedClassCode = await _unitOfWork.ClassRepository.Get(r => r.ClassCode.Equals(newEntity.ClassCode)).FirstOrDefaultAsync();
            if (existedClassCode is not null)
            {
                return new ServiceResponseVM<Class>
                {
                    IsSuccess = false,
                    Title = "Create Class failed",
                    Errors = new string[1] { "Class Code is already exist" }
                };

            }
            var existedSemester = await _unitOfWork.SemesterRepository.Get(r => r.SemesterCode.Equals(newEntity.SemesterCode)).FirstOrDefaultAsync();
            if (existedSemester is null)
            {
                return new ServiceResponseVM<Class>
                {
                    IsSuccess = false,
                    Title = "Create Class failed",
                    Errors = new string[1] { "Semester Code is not exist" }
                };

            }

            var existedSubject = await _unitOfWork.SubjectRepository.Get(r => r.SubjectCode.Equals(newEntity.SubjectCode)).FirstOrDefaultAsync();
            if (existedSubject is null)
            {
                return new ServiceResponseVM<Class>
                {
                    IsSuccess = false,
                    Title = "Create Class failed",
                    Errors = new string[1] { "Subject Code is not exist" }
                };

            }

            Class newClass = new Class()
            {
                ClassCode = newEntity.ClassCode,
                ClassStatus = 1,
                SemesterID = existedSemester.SemesterID,
                RoomID = existedRoom.RoomID,
                SubjectID = existedSubject.SubjectID,
                LecturerID = newEntity.LecturerID,
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

        public async Task<IEnumerable<Class>> Get(int startPage, int endPage, Guid? lecturerId, int quantity, int? semesterId, string? classCode)
        {
            int quantityResult = 0;
            _validateGet.ValidateGetRequest(ref startPage, ref endPage, quantity, ref quantityResult);
            if (quantityResult == 0)
            {
                throw new ArgumentException("Error when get quantity per page");
            }
            
            var query = await _unitOfWork.ClassRepository.FindAll().Include(c => c.Lecturer).Include(c => c.Semester).ToArrayAsync();

            if (lecturerId.HasValue)
            {
                query = query.Where(c => c.LecturerID == lecturerId).ToArray();
            }

            if (semesterId.HasValue)
            {
                query = query.Where(c => c.SemesterID == semesterId).ToArray();
            }

            if (classCode != null)
            {
                query = query.Where(c => c.ClassCode.Equals(classCode)).ToArray();
            }

            var classes = query
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult);

            return classes;
        }

        public async Task<Class> GetClassDetail(int scheduleID)
        {
            var includes = new Expression<Func<Class, object?>>[]
        {
            c => c.Lecturer,
            c => c.Subject,
            c => c.Schedules,
            c => c.Room,
        };
            var classDetail = await _unitOfWork.ClassRepository.Get(cl => cl.Schedules.Any(c => c.ScheduleID == scheduleID),includes: includes).SingleOrDefaultAsync();
            if(classDetail == null)
            {
                throw new Exception($"Not Found Class With ScheduleID = {scheduleID}");
            }
            return classDetail;
        }
    }
}
