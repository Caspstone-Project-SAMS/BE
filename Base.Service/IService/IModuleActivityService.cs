using Base.Repository.Entity;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IModuleActivityService
{
    Task<ServiceResponseVM<ModuleActivity>> Create(ActivityHistoryVM newEntity);
    Task<ServiceResponseVM<IEnumerable<ModuleActivity>>> GetAll(int startPage, int endPage, int quantity, string? title, string? description, Guid? userId, DateTime? activityDate, bool? IsSuccess, int? moduleId, int? scheduleId);
    Task<ModuleActivity?> GetById(int id);
}
