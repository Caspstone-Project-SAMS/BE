using Base.Repository.Common;
using Base.Repository.Entity;
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

namespace Base.Service.Service;

internal class NotificationTypeService : INotificationTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;

    public NotificationTypeService(IUnitOfWork unitOfWork, IValidateGet validateGet)
    {
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
    }

    public async Task<ServiceResponseVM<NotificationType>> Create(NotificationTypeVM newEntity)
    {
        var existedNotification = await _unitOfWork.NotificationTypeRepository
            .Get(n => !n.IsDeleted && n.TypeName == newEntity.TypeName)
            .FirstOrDefaultAsync();
        if(existedNotification is not null)
        {
            return new ServiceResponseVM<NotificationType>
            {
                IsSuccess = false,
                Title = "Create notification type failed",
                Errors = new string[1] { "Type name is already existed" }
            };
        }

        var newNotificationType = new NotificationType
        {
            TypeName = newEntity.TypeName,
            TypeDescription = newEntity.TypeDescription,
        };

        try
        {
            await _unitOfWork.NotificationTypeRepository.AddAsync(newNotificationType);

            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                return new ServiceResponseVM<NotificationType>
                {
                    IsSuccess = true,
                    Title = "Create notification type successfully",
                    Result = newNotificationType
                };
            }
            else
            {
                return new ServiceResponseVM<NotificationType>
                {
                    IsSuccess = false,
                    Title = "Create notification type failed",
                };
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponseVM<NotificationType>
            {
                IsSuccess = false,
                Title = "Create notification type failed",
                Errors = new string[1] { ex.Message }
            };
        }
        catch (OperationCanceledException ex)
        {
            return new ServiceResponseVM<NotificationType>
            {
                IsSuccess = false,
                Title = "Create notification type failed",
                Errors = new string[2] { "The operation has been cancelled", ex.Message }
            };
        }
    }

    public async Task<ServiceResponseVM<IEnumerable<NotificationType>>> GetAll(int startPage, int endPage, int quantity, string? typeName, string? typeDescription)
    {
        var result = new ServiceResponseVM<IEnumerable<NotificationType>>()
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
        ParameterExpression pe = Expression.Parameter(typeof(NotificationType), "n");
        MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        if (containsMethod is null)
        {
            errors.Add("Method Contains can not found from string type");
            return result;
        }

        expressions.Add(Expression.Equal(Expression.Property(pe, nameof(NotificationType.IsDeleted)), Expression.Constant(false)));

        if(typeName is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(NotificationType.TypeName)), Expression.Constant(typeName)));
        }

        if(typeDescription is not null)
        {
            expressions.Add(Expression.Call(containsMethod, Expression.Property(pe, nameof(NotificationType.TypeDescription)), Expression.Constant(typeDescription)));
        }

        Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
        Expression<Func<NotificationType, bool>> where = Expression.Lambda<Func<NotificationType, bool>>(combined, pe);

        var notificationTypes = await _unitOfWork.NotificationTypeRepository
            .Get(where)
            .AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult)
            .ToArrayAsync();

        result.IsSuccess = true;
        result.Result = notificationTypes;
        result.Title = "Get successfully";

        return result;
    }

    public async Task<NotificationType?> GetById(int notificationTypeId)
    {
        var includes = new Expression<Func<NotificationType, object?>>[]
        {
            n => n.Notifications
        };
        return await _unitOfWork.NotificationTypeRepository
            .Get(n => !n.IsDeleted && n.NotificationTypeID == notificationTypeId, includes)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
