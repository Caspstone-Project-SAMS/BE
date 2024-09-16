using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Google.Api.Gax;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class ImportSchedulesRecordService : IImportSchedulesRecordService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;

    public ImportSchedulesRecordService(IUnitOfWork unitOfWork, IValidateGet validateGet)
    {
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
    }

    public async Task<ServiceResponseVM<IEnumerable<ImportSchedulesRecord>>> GetAllRecord(int startPage, int endPage, int quantity, Guid? userId)
    {
        var result = new ServiceResponseVM<IEnumerable<ImportSchedulesRecord>>()
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
        ParameterExpression pe = Expression.Parameter(typeof(ImportSchedulesRecord), "i");

        expressions.Add(Expression.Constant(true));

        if(userId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(ImportSchedulesRecord.UserId)), Expression.Constant(userId)));
        }

        Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
        Expression<Func<ImportSchedulesRecord, bool>> where = Expression.Lambda<Func<ImportSchedulesRecord, bool>>(combined, pe);

        var includes = new Expression<Func<ImportSchedulesRecord, object?>>[]
        {
            i => i.User
        };

        var importedSchedulesRecored = await _unitOfWork.ImportSchedulesRecordRepository
            .Get(where, includes)
            .AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult)
            .ToArrayAsync();

        var revertableDuration = _unitOfWork.SystemConfigurationRepository.Get(s => true).FirstOrDefault()?.RevertableDurationInHours ?? 12;
        foreach(var record in importedSchedulesRecored)
        {
            record.IsReversible = (record.RecordTimestamp.AddHours(revertableDuration) < ServerDateTime.GetVnDateTime()) ? false : true;
        }

        result.IsSuccess = true;
        result.Result = importedSchedulesRecored;
        result.Title = "Get records successfully";

        return result;
    }

    public async Task<ServiceResponseVM> RevertRecords(int restoredRecord)
    {
        var existedRecord = _unitOfWork.ImportSchedulesRecordRepository
            .Get(r => r.ImportSchedulesRecordID == restoredRecord, new Expression<Func<ImportSchedulesRecord, object?>>[]
            {
                i => i.ImportedSchedules
            })
            .Include(nameof(ImportSchedulesRecord.ImportedSchedules) + "." + nameof(Schedule.Attendances))
            .Include(nameof(ImportSchedulesRecord.ImportedSchedules) + "." + nameof(Schedule.PreparedSchedules))
            .Include(nameof(ImportSchedulesRecord.ImportedSchedules) + "." + nameof(Schedule.SubstituteTeaching))
            .FirstOrDefault();

        if(existedRecord is null)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Revert record failed",
                Errors = new string[1] { "Record not found" }
            };
        }

        if (existedRecord.ImportReverted)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Revert record failed",
                Errors = new string[1] { "Record is already reverted" }
            };
        }

        var revertableDuration = _unitOfWork.SystemConfigurationRepository.Get(s => true).FirstOrDefault()?.RevertableDurationInHours ?? 12;
        if(existedRecord.RecordTimestamp.AddHours(revertableDuration) < ServerDateTime.GetVnDateTime())
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Revert record failed",
                Errors = new string[2] { "Record can not be reverted", $"Record can only be reverted within {revertableDuration} hours after created" }
            };
        }

        if(existedRecord.ImportedSchedules.Count() == 0)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Revert record failed",
                Errors = new string[1] { "Record does not have any schedule" }
            };
        }
        // Chưa check notification
        // Lets revert record
        foreach(var schedule in existedRecord.ImportedSchedules)
        { 
            if(schedule.SubstituteTeaching is null && schedule.PreparedSchedules.Count() <= 0)
            {
                _unitOfWork.AttendanceRepository.RemoveRange(schedule.Attendances);
                _unitOfWork.ScheduleRepository.Remove(schedule);
            }
        }

        existedRecord.ImportReverted = true;

        try
        {
            var result = await _unitOfWork.SaveChangesAsync();
            if (result)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = true,
                    Title = "Revert record successfully"
                };
            }
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Revert record failed",
                Errors = new string[1] { "Error when saving changes" }
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Revert record failed",
                Errors = new List<string> { "Error when saving changes", ex.Message }
            };
        }
    }
}
