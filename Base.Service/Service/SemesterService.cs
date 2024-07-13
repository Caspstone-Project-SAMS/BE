using Base.IService.IService;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.RequestVM.Role;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class SemesterService : ISemesterService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;
        public SemesterService(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
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

            Semester newSemester =  new Semester
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
            var existedSemester = await _unitOfWork.SemesterRepository.Get(s => s.SemesterID == id).SingleOrDefaultAsync();
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
                var checkSemesterCode =  _unitOfWork.SemesterRepository.Get(s => s.SemesterCode == updateSemester.SemesterCode).FirstOrDefault() is not null;
                if (checkSemesterCode)
                {
                    return new ServiceResponseVM<Semester>
                    {
                        IsSuccess = false,
                        Title = "Update Semester failed",
                        Errors = new string[1] { $"Semester Code {updateSemester.SemesterCode} is already taken" }
                    };
                }
                if (updateSemester.StartDate != existedSemester.StartDate || updateSemester.EndDate != existedSemester.EndDate)
                {
                    var overlappingSemester = await _unitOfWork.SemesterRepository.Get(s =>
                (updateSemester.StartDate >= s.StartDate && updateSemester.StartDate <= s.EndDate) ||
                (updateSemester.EndDate >= s.StartDate && updateSemester.EndDate <= s.EndDate) ||
                (s.StartDate >= updateSemester.StartDate && s.StartDate <= updateSemester.EndDate) ||
                (s.EndDate >= updateSemester.StartDate && s.EndDate <= updateSemester.EndDate)).AnyAsync();

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
                
            }

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
    }
}
