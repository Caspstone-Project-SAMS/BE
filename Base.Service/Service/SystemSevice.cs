using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class SystemSevice : ISystemService
{
    private readonly IUnitOfWork _unitOfWork;
    public SystemSevice(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<SystemConfiguration> GetSystemConfiguration()
    {
        var result = await _unitOfWork.SystemConfigurationRepository
            .Get(s => true)
            .AsNoTracking()
            .FirstOrDefaultAsync();
        if(result is not null)
        {
            return result;
        }
        return new SystemConfiguration()
        {
            RevertableDurationInHours = 0,
            ClassCodeMatchRate = 0,
            SemesterDurationInDays = 0,
            SlotDurationInMins = 0
        };
    }

    public async Task<ServiceResponseVM<SystemConfiguration>> UpdateConfiguration(SystemConfigurationVM resource)
    {
        var result = new ServiceResponseVM<SystemConfiguration>()
        {
            IsSuccess = false
        };
        var errors = new List<string>();

        if(resource.RevertableDurationInHours is not null && resource.RevertableDurationInHours < 0)
        {
            errors.Add("Value of revertable duration must be greater then or equal 0");
        }
        if (resource.ClassCodeMatchRate is not null && resource.ClassCodeMatchRate < 0)
        {
            errors.Add("Value of class code match rate must be greater then or equal 0");
        }
        if (resource.SemesterDurationInDays is not null && resource.SemesterDurationInDays < 0)
        {
            errors.Add("Value of semester duration must be greater then or equal 0");
        }
        if (resource.SlotDurationInMins is not null && resource.SlotDurationInMins < 0)
        {
            errors.Add("Value of slot duration must be greater then or equal 0");
        }

        if(errors.Count > 0)
        {
            result.IsSuccess = false;
            result.Title = "Update system configuration failed";
            result.Errors = errors;
            return result;
        }

        var existedConfiguration = _unitOfWork.SystemConfigurationRepository
            .Get(s => true)
            .FirstOrDefault();

        if(existedConfiguration is null)
        {
            // Let's initialize new one
            existedConfiguration = new SystemConfiguration()
            {
                RevertableDurationInHours = resource.RevertableDurationInHours ?? 0,
                ClassCodeMatchRate = resource.ClassCodeMatchRate ?? 0,
                SemesterDurationInDays = resource.SemesterDurationInDays ?? 0,
                SlotDurationInMins = resource.SlotDurationInMins ?? 0
            };
            await _unitOfWork.SystemConfigurationRepository.AddAsync(existedConfiguration);
        }
        else
        {
            var copyConfiguration = (SystemConfiguration)existedConfiguration.Clone();

            existedConfiguration.RevertableDurationInHours = resource.RevertableDurationInHours is null ? existedConfiguration.RevertableDurationInHours : resource.RevertableDurationInHours ?? 0;
            existedConfiguration.ClassCodeMatchRate = resource.ClassCodeMatchRate is null ? existedConfiguration.ClassCodeMatchRate : resource.ClassCodeMatchRate ?? 0;
            existedConfiguration.SemesterDurationInDays = resource.SemesterDurationInDays is null ? existedConfiguration.SemesterDurationInDays : resource.SemesterDurationInDays ?? 0;
            existedConfiguration.SlotDurationInMins = resource.SlotDurationInMins is null ? existedConfiguration.SlotDurationInMins : resource.SlotDurationInMins ?? 0;

            if (TwoConfigurationsAreSame(copyConfiguration, existedConfiguration))
            {
                result.IsSuccess = true;
                result.Title = "Update system configuration successfully";
                result.Result = existedConfiguration;
                return result;
            }
        }

        try
        {
            var finalResult = await _unitOfWork.SaveChangesAsync();
            if (finalResult)
            {
                result.IsSuccess = true;
                result.Result = existedConfiguration;
                result.Title = "Update system configuration successfully";
                return result;
            }

            result.IsSuccess = false;
            result.Title = "Update system configuration failed";
            result.Errors = new string[1] { "Error when saving changes" };
            return result;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Title = "Update system configuration failed";
            result.Errors = new string[2] { "Error when saving changes", ex.Message };
            return result;
        }
    }

    private bool TwoConfigurationsAreSame(SystemConfiguration object1, SystemConfiguration object2)
    {
        return (object1.RevertableDurationInHours == object2.RevertableDurationInHours && object1.ClassCodeMatchRate == object2.ClassCodeMatchRate &&
            object1.SemesterDurationInDays == object2.SemesterDurationInDays && object1.SlotDurationInMins == object2.SlotDurationInMins);
    }
}
