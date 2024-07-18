using Base.Repository.Identity;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IEmployeeService
{
    Task<User?> GetById(Guid id);
    Task<ServiceResponseVM<IEnumerable<User>>> GetAll(int startPage, int endPage, int quantity, string? email, string? phone, string? department, int? roleId);
}
