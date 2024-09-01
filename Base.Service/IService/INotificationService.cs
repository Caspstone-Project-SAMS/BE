using Base.Repository.Entity;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface INotificationService
{
    Task<ServiceResponseVM<IEnumerable<Notification>>> GetAll(int startPage, int endPage, int quantity, bool? read, Guid? userId, int? notificationTypeId);
    Task<Notification?> GetById(int notificationId);
    Task<ServiceResponseVM<Notification>> Create(NotificationVM newEntity);
    Task ReadNotifications(IEnumerable<int> notificationsIds);
}
