using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class ActivityHistoryService : IActivityHistoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;

    public ActivityHistoryService(IUnitOfWork unitOfWork, IValidateGet validateGet)
    {
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
    }

    public async Task<ServiceResponseVM<ActivityHistory>> Create(ActivityHistoryVM newEntity)
    {
        var existedModule = await _unitOfWork.ModuleRepository
            .Get(m => !m.IsDeleted && m.ModuleID == newEntity.ModuleID)
            .FirstOrDefaultAsync();
        if( existedModule is null )
        {
            return new ServiceResponseVM<ActivityHistory>
            {
                IsSuccess = false,
                Title = "Create activity history failed",
                Errors = new string[1] { "Module not found" }
            };
        }

        var newActivityHistory = new ActivityHistory
        {
            Title = newEntity.Title,
            Description = newEntity.Description,
            UserId = newEntity.UserId,
            StartTime = newEntity.StartTime,
            EndTime = newEntity.EndTime,
            IsSuccess = newEntity.IsSuccess,
            Errors = String.Join(";", newEntity.Errors),
            ModuleID = newEntity.ModuleID
        };

        if(newEntity.PreparationTaskVM is not null)
        {
            var newPreparationTask = new PreparationTask
            {
                Progress = newEntity.PreparationTaskVM.Progress,
                PreparedScheduleId = newEntity.PreparationTaskVM.PreparedScheduleId,
                PreparedSchedules = String.Join(";", newEntity.PreparationTaskVM.PreparedScheduleIds),
                PreparedDate = newEntity.PreparationTaskVM.PreparedDate
            };
            newActivityHistory.PreparationTask = newPreparationTask;
        }

        try
        {
            await _unitOfWork.ActivityHistoryRepository.AddAsync(newActivityHistory);

            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                return new ServiceResponseVM<ActivityHistory>
                {
                    IsSuccess = true,
                    Title = "Create activity history successfully",
                    Result = newActivityHistory
                };
            }
            else
            {
                return new ServiceResponseVM<ActivityHistory>
                {
                    IsSuccess = false,
                    Title = "Create activity history failed",
                };
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponseVM<ActivityHistory>
            {
                IsSuccess = false,
                Title = "Create activity history failed",
                Errors = new string[1] { ex.Message }
            };
        }
        catch (OperationCanceledException ex)
        {
            return new ServiceResponseVM<ActivityHistory>
            {
                IsSuccess = false,
                Title = "Create activity history failed",
                Errors = new string[2] { "The operation has been cancelled", ex.Message }
            };
        }
    }
}
