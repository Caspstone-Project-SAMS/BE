using Base.Repository.Identity;
using Base.Service.ViewModel.RequestVM.Role;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IRoleService
{
    Task<Role?> GetById(Guid id);
    Task<IEnumerable<Role>> Get(int startPage, int endPage, int? quantity, string? roleName);
    Task<ServiceResponseVM<Role>> Create(Role newEntity);
    Task<ServiceResponseVM> Delete(Guid id);
    Task<ServiceResponseVM<Role>> Update(Role updateRole, Guid id);
}
