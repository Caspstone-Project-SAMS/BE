using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.IService;
using Base.Service.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;
    public EmployeeService(IUnitOfWork unitOfWork, IValidateGet validateGet)
    {
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
    }

    public async Task<User?> GetById(Guid id)
    {
        var includes = new Expression<Func<User, object?>>[]
        {
            u => u.ManagedClasses,
            u => u.Employee!.Modules
        };
        return await _unitOfWork.UserRepository
            .Get(u => u.EmployeeID == id, includes)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }
}
