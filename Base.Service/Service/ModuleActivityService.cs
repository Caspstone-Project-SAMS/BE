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
            int? preparedScheduleId = null;
            if (newEntity.PreparationTaskVM.PreparedScheduleId is not null)
            {
                var checkExistedSchedule = _unitOfWork.ScheduleRepository
                    .Get(s => !s.IsDeleted && s.ScheduleID == newEntity.PreparationTaskVM.PreparedScheduleId)
                    .AsNoTracking()
                    .FirstOrDefault();
                preparedScheduleId = checkExistedSchedule?.ScheduleID;
            }

            var preparedSchedules = new List<PreparedSchedule>();
            foreach(var preparedSchedule in newEntity.PreparationTaskVM.PreparedSchedules)
            {
                preparedSchedules.Add(new PreparedSchedule
                {
                    ScheduleID = preparedSchedule.ScheduleId,
                    TotalFingerprints = preparedSchedule.TotalFingers,
                    UploadedFingerprints = preparedSchedule.UploadedFingers
                });
            }


            var newPreparationTask = new PreparationTask
            {
                Progress = newEntity.PreparationTaskVM.Progress,
                PreparedScheduleId = preparedScheduleId,
                PreparedSchedules = preparedSchedules,
                PreparedDate = newEntity.PreparationTaskVM.PreparedDate,
                TotalFingers = newEntity.PreparationTaskVM.TotalFingers,
                UploadedFingers = newEntity.PreparationTaskVM.UploadedFingers
            };

            newActivityHistory.PreparationTask = newPreparationTask;
            await _unitOfWork.PreparationTaskRepository.AddAsync(newPreparationTask);
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

    public async Task<ServiceResponseVM<IEnumerable<ModuleActivity>>> GetAll(
        int startPage, 
        int endPage, 
        int quantity, 
        string? title, 
        string? description, 
        Guid? userId, 
        DateTime? activityDate, 
        bool? IsSuccess, 
        int? moduleId,
        int? scheduleId)
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
        MethodInfo? anyMethodOfList = typeof(Enumerable).GetMethods()
            .FirstOrDefault(m => m.Name == "Any" && m.GetParameters().Length == 2)?
            .MakeGenericMethod(typeof(PreparedSchedule));

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
        if(anyMethodOfList is null)
        {
            errors.Add("Any method of list not found");
            return result;
        }

        expressions.Add(Expression.Constant(true));

        if(title is not null)
        {
            expressions.Add(Expression.Call(Expression.Property(pe, nameof(ModuleActivity.Title)), containsMethod, Expression.Constant(title)));
        }

        if(description is not null)
        {
            expressions.Add(Expression.Call(Expression.Property(pe, nameof(ModuleActivity.Description)), containsMethod, Expression.Constant(description)));
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

        if(scheduleId is not null)
        {
            var preparationTaskProperty = Expression.Property(pe, "PreparationTask");
            var preparedScheduleIdProperty = Expression.Property(preparationTaskProperty, "PreparedScheduleId");

            // Check for null and then compare
            var hasValueExpression = Expression.Property(preparedScheduleIdProperty, "HasValue");
            var valueExpression = Expression.Property(preparedScheduleIdProperty, "Value");

            var firstExpression = Expression.AndAlso(
                hasValueExpression,
                Expression.Equal(valueExpression, Expression.Constant(scheduleId))
            );

            var preparedSchedulesProperty = Expression.Property(preparationTaskProperty, "PreparedSchedules");

            var preparedScheduleParameter = Expression.Parameter(typeof(PreparedSchedule), "s");
            var preparedSchedulesIdProperty = Expression.Property(preparedScheduleParameter, "ScheduleID");
            var preparedSchedulesIdCondition = Expression.Equal(preparedSchedulesIdProperty, Expression.Constant(scheduleId));
            var anyLambda = Expression.Lambda(preparedSchedulesIdCondition, preparedScheduleParameter);

            var secondExpression = Expression.Call(anyMethodOfList, Expression.Property(preparationTaskProperty, "PreparedSchedules"), anyLambda);

            expressions.Add(Expression.Or(firstExpression, secondExpression));
        }

        var includes = new Expression<Func<ModuleActivity, object?>>[]
        {
            m => m.Module,
            m => m.PreparationTask!.PreparedSchedules
        };

        Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
        Expression<Func<ModuleActivity, bool>> where = Expression.Lambda<Func<ModuleActivity, bool>>(combined, pe);
        var moduleActivities = await _unitOfWork.ModuleActivityRepository
                .Get(where, includes)
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
            m => m.PreparationTask!.PreparedSchedules
        };
        return await _unitOfWork.ModuleActivityRepository
            .Get(m => m.ModuleActivityId == id, includes)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
