using AutoMapper.QueryableExtensions;
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
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class ModuleActivityService : IModuleActivityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;

    public ModuleActivityService(IUnitOfWork unitOfWork, IValidateGet validateGet)
    {
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
    }

    public async Task<ServiceResponseVM<ModuleActivity>> Create(ActivityHistoryVM newEntity)
    {
        var existedModule = await _unitOfWork.ModuleRepository
            .Get(m => !m.IsDeleted && m.ModuleID == newEntity.ModuleID)
            .FirstOrDefaultAsync();
        if( existedModule is null )
        {
            return new ServiceResponseVM<ModuleActivity>
            {
                IsSuccess = false,
                Title = "Create activity history failed",
                Errors = new string[1] { "Module not found" }
            };
        }

        var newActivityHistory = new ModuleActivity
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
            await _unitOfWork.ModuleActivityRepository.AddAsync(newActivityHistory);

            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                return new ServiceResponseVM<ModuleActivity>
                {
                    IsSuccess = true,
                    Title = "Create activity history successfully",
                    Result = newActivityHistory
                };
            }
            else
            {
                return new ServiceResponseVM<ModuleActivity>
                {
                    IsSuccess = false,
                    Title = "Create activity history failed",
                };
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponseVM<ModuleActivity>
            {
                IsSuccess = false,
                Title = "Create activity history failed",
                Errors = new string[1] { ex.Message }
            };
        }
        catch (OperationCanceledException ex)
        {
            return new ServiceResponseVM<ModuleActivity>
            {
                IsSuccess = false,
                Title = "Create activity history failed",
                Errors = new string[2] { "The operation has been cancelled", ex.Message }
            };
        }
    }

    public async Task<ServiceResponseVM<IEnumerable<ModuleActivity>>> GetAll(int startPage, int endPage, int quantity, string? title, string? description, Guid? userId, DateTime? activityDate, bool? IsSuccess, int? moduleId)
    {
        var result = new ServiceResponseVM<IEnumerable<ModuleActivity>>()
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
        ParameterExpression pe = Expression.Parameter(typeof(ModuleActivity), "m");
        MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        MethodInfo? toStringMethodOfDateTime = typeof(DateTime).GetMethod(nameof(DateTime.ToString), new[] { typeof(string) });
        if (containsMethod is null)
        {
            errors.Add("Method Contains can not found from string type");
            return result;
        }
        if(toStringMethodOfDateTime is null)
        {
            errors.Add("Method ToString can not found from DateTime type");
            return result;
        }

        expressions.Add(Expression.Constant(true));

        if(title is not null)
        {
            expressions.Add(Expression.Call(containsMethod, Expression.Property(pe, nameof(ModuleActivity.Title)), Expression.Constant(title)));
        }

        if(description is not null)
        {
            expressions.Add(Expression.Call(containsMethod, Expression.Property(pe, nameof(ModuleActivity.Description)), Expression.Constant(description)));
        }

        if(userId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(ModuleActivity.UserId)), Expression.Constant(userId)));
        }

        if(IsSuccess is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(ModuleActivity.IsSuccess)), Expression.Constant(IsSuccess)));
        }

        if(moduleId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(ModuleActivity.ModuleID)), Expression.Constant(moduleId)));
        }

        Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
        Expression<Func<ModuleActivity, bool>> where = Expression.Lambda<Func<ModuleActivity, bool>>(combined, pe);
        var moduleActivities = await _unitOfWork.ModuleActivityRepository
                .Get(where)
                .AsNoTracking()
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult)
                .ToListAsync();

        if(activityDate is not null)
        {
            moduleActivities = moduleActivities.Where(a => a.StartTime.ToString("yyyy-MM-dd") == activityDate.Value.ToString("yyyy-MM-dd")).ToList();
        }

        result.IsSuccess = true;
        result.Result = moduleActivities;
        result.Title = "Get successfully";

        return result;
    }

    public async Task<ModuleActivity?> GetById(int id)
    {
        var includes = new Expression<Func<ModuleActivity, object?>>[]
        {
            m => m.Module,
            m => m.PreparationTask
        };
        return await _unitOfWork.ModuleActivityRepository
            .Get(m => m.ModuleActivityId == id, includes)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
