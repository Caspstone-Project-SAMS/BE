using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class SubjectService : ISubjectService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public SubjectService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }
        public async Task<ServiceResponseVM<Subject>> Create(SubjectVM newEntity)
        {
            var existedSubject = await _unitOfWork.SubjectRepository.Get(st => st.SubjectCode.Equals(newEntity.SubjectCode)).SingleOrDefaultAsync();
            if (existedSubject is not null)
            {
                return new ServiceResponseVM<Subject>
                {
                    IsSuccess = false,
                    Title = "Create Subject failed",
                    Errors = new string[1] { $"Subject Code {newEntity.SubjectCode} is already taken" }
                };

            }

            Subject newSubject = new Subject
            {
                SubjectCode = newEntity.SubjectCode,
                SubjectName = newEntity.SubjectName,
                SubjectStatus = newEntity.SubjectStatus,
                CreatedBy = _currentUserService.UserId,
                CreatedAt = ServerDateTime.GetVnDateTime(),
            };

            try
            {
                await _unitOfWork.SubjectRepository.AddAsync(newSubject);

                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponseVM<Subject>
                    {
                        IsSuccess = true,
                        Title = "Create Subject successfully",
                        Result = newSubject
                    };
                }
                else
                {
                    return new ServiceResponseVM<Subject>
                    {
                        IsSuccess = false,
                        Title = "Create Subject failed",
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM<Subject>
                {
                    IsSuccess = false,
                    Title = "Create Subject failed",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM<Subject>
                {
                    IsSuccess = false,
                    Title = "Create Subject failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }

        public async Task<ServiceResponseVM> Delete(int id)
        {
            var existedSubject = await _unitOfWork.SubjectRepository.Get(r => r.SubjectID == id && !r.IsDeleted).FirstOrDefaultAsync();
            if (existedSubject is null)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete subject failed",
                    Errors = new string[1] { "Subject not found" }
                };
            }

            existedSubject.IsDeleted = true;
            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = true,
                        Title = "Delete subject successfully"
                    };
                }
                else
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = false,
                        Title = "Delete subject failed",
                        Errors = new string[1] { "Save changes failed" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete subject failed",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete subject failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }

        public async Task<IEnumerable<Subject>> Get()
        {
            return await _unitOfWork.SubjectRepository.Get(s => s.IsDeleted == false).ToArrayAsync();
        }

        public async Task<Subject?> GetById(int id)
        {
            return await _unitOfWork.SubjectRepository.Get(s => s.SubjectID == id && s.IsDeleted == false).FirstOrDefaultAsync();
        }

        public async Task<ServiceResponseVM<Subject>> Update(SubjectVM updateEntity, int id)
        {
                var existedSubject = await _unitOfWork.SubjectRepository.Get(s => s.SubjectID == id).SingleOrDefaultAsync();
                if (existedSubject is null)
                {
                    return new ServiceResponseVM<Subject>
                    {
                        IsSuccess = false,
                        Title = "Update Subject failed",
                        Errors = new string[1] { "Subject not found" }
                    };
                }

                if (updateEntity.SubjectCode != existedSubject.SubjectCode)
                {
                    var checkSubjectCode = _unitOfWork.SubjectRepository.Get(s => s.SubjectCode == updateEntity.SubjectCode).FirstOrDefault() is not null;
                    if (checkSubjectCode)
                    {
                        return new ServiceResponseVM<Subject>
                        {
                            IsSuccess = false,
                            Title = "Update Subject failed",
                            Errors = new string[1] { $"Subject Code {updateEntity.SubjectCode} is already taken" }
                        };
                    }
                }
                existedSubject.SubjectCode = updateEntity.SubjectCode!;
                existedSubject.SubjectName = updateEntity.SubjectName;
                existedSubject.SubjectStatus = updateEntity.SubjectStatus;

                _unitOfWork.SubjectRepository.Update(existedSubject);
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponseVM<Subject>
                    {
                        IsSuccess = true,
                        Title = "Update Subject successfully",
                        Result = existedSubject
                    };
                }
                else
                {
                    return new ServiceResponseVM<Subject>
                    {
                        IsSuccess = false,
                        Title = "Update Subject failed"
                    };
                }
            }
        }
    }

