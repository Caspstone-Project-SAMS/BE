using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.Common;
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

    public async Task<ServiceResponseVM<Notification>> Create(NotificationVM newEntity)
    {
        var existedUser = await _unitOfWork.UserRepository.Get(u => !u.Deleted && u.Id == newEntity.UserID).FirstOrDefaultAsync();
        if(existedUser is null)
        {
            return new ServiceResponseVM<Notification>
            {
                IsSuccess = false,
                Title = "Create notification failed",
                Errors = new string[1] { "User not found" }
            };
        }

        var existedNotificationtype = await _unitOfWork.NotificationTypeRepository.Get(n => n.NotificationTypeID == newEntity.NotificationTypeID).FirstOrDefaultAsync();
        if(existedNotificationtype is null)
        {
            return new ServiceResponseVM<Notification>
            {
                IsSuccess = false,
                Title = "Create notification failed",
                Errors = new string[1] { "Notification type not found" }
            };
        }

        var newNotification = new Notification
        {
            Title = newEntity.Title,
            Description = newEntity.Description,
            TimeStamp = ServerDateTime.GetVnDateTime(),
            Read = newEntity.Read,
            UserID = newEntity.UserID,
            NotificationTypeID = newEntity.NotificationTypeID
        };

        try
        {
            await _unitOfWork.NotificationRepository.AddAsync(newNotification);

            var result = await _unitOfWork.SaveChangesAsync();

            if (result)
            {
                return new ServiceResponseVM<Notification>
                {
                    IsSuccess = true,
                    Title = "Create notification successfully",
                    Result = newNotification
                };
            }
            else
            {
                return new ServiceResponseVM<Notification>
                {
                    IsSuccess = false,
                    Title = "Create notification failed",
                };
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponseVM<Notification>
            {
                IsSuccess = false,
                Title = "Create notification failed",
                Errors = new string[1] { ex.Message }
            };
        }
        catch (OperationCanceledException ex)
        {
            return new ServiceResponseVM<Notification>
            {
                IsSuccess = false,
                Title = "Create notification failed",
                Errors = new string[2] { "The operation has been cancelled", ex.Message }
            };
        }
    }
}
