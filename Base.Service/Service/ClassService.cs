﻿using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.Common;
using Base.Service.IService;
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
        public ClassService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                CreatedBy = newEntity.CreatedBy,
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
