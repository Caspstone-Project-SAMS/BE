using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class NotificationService : INotificationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;
    public NotificationService(IUnitOfWork unitOfWork, IValidateGet validateGet)
    {
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
    }

    public async Task<ServiceResponseVM<IEnumerable<Notification>>> GetAll(int startPage, int endPage, int quantity, bool? read, Guid? userId, int? notificationTypeId)
    {
        var result = new ServiceResponseVM<IEnumerable<Notification>>()
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
        ParameterExpression pe = Expression.Parameter(typeof(Notification), "n");
        MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        if (containsMethod is null)
        {
            errors.Add("Method Contains can not found from string type");
            return result;
        }

        expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Notification.IsDeleted)), Expression.Constant(false)));

        if(read is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Notification.Read)), Expression.Constant(read)));
        }

        if(userId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Notification.UserID)), Expression.Constant(userId)));
        }

        if(notificationTypeId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Notification.NotificationTypeID)), Expression.Constant(notificationTypeId)));
        }

        Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
        Expression<Func<Notification, bool>> where = Expression.Lambda<Func<Notification, bool>>(combined, pe);

        var includes = new Expression<Func<Notification, object?>>[]
        {
            n => n.NotificationType
        };

        var notifications = await _unitOfWork.NotificationRepository
            .Get(where, includes)
            .AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult)
            .ToArrayAsync();

        result.IsSuccess = true;
        result.Result = notifications;
        result.Title = "Get successfully";

        return result;
    }

    public async Task<Notification?> GetById(int notificationId)
    {
        return await _unitOfWork.NotificationRepository.Get(n => !n.IsDeleted && n.NotificationID == notificationId).FirstOrDefaultAsync();
    }
}
