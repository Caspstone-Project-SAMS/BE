using Base.Repository.Entity;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface INotificationTypeService
{
    Task<ServiceResponseVM<NotificationType>> Create(NotificationTypeVM newEntity);
    Task<ServiceResponseVM<IEnumerable<NotificationType>>> GetAll(int startPage, int endPage, int quantity, string? typeName, string? typeDescription);
    Task<NotificationType?> GetById(int notificationTypeId);
}
