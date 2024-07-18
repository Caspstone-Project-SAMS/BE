using Base.Repository.Entity;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IModuleService
{
    Task<Module?> GetById(int moduleId);
    Task<ServiceResponseVM<IEnumerable<Module>>> Get(int startPage, int endPage, int? quantity, int? mode, int? status, string? key, Guid? employeeId);
    Task<ServiceResponseVM<Module>> Update();

}
