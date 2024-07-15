using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.ResponseVM;
using FirebaseAdmin;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Module = Base.Repository.Entity.Module;

namespace Base.Service.Service;

internal class ModuleService : IModuleService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;
    public ModuleService(IUnitOfWork unitOfWork, IValidateGet validateGet)
    {
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
    }

    public async Task<Module?> GetById(int moduleId)
    {
        var includes = new Expression<Func<Module, object?>>[]
        {
            m => m.Employee!.User
        };
        return await _unitOfWork.ModuleRepository
            .Get(m => m.ModuleID == moduleId, includes)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResponseVM<IEnumerable<Module>>> Get(int startPage, int endPage, int? quantity, int? mode, int? status, string? key, Guid? employeeId)
    {
        var result = new ServiceResponseVM<IEnumerable<Module>>()
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
        ParameterExpression pe = Expression.Parameter(typeof(Module), "m");
        MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        if (containsMethod is null)
        {
            errors.Add("Method Contains can not found from string type");
            return result;
        }

        expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Module.IsDeleted)), Expression.Constant(false)));

        if(mode is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Module.Mode)), Expression.Constant(mode)));
        }

        if(status is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Module.Status)), Expression.Constant(status)));
        }

        if(key is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Module.Key)), Expression.Constant(key)));
        }

        if(employeeId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Module.EmployeeID)), Expression.Constant(employeeId)));
        }

        Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
        Expression<Func<Module, bool>> where = Expression.Lambda<Func<Module, bool>>(combined, pe);

        var includes = new Expression<Func<Module, object?>>[]
        {
            m => m.Employee!.User
        };

        var modules = await _unitOfWork.ModuleRepository
            .Get(where, includes)
            .AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult)
            .ToArrayAsync();

        result.IsSuccess = true;
        result.Result = modules;
        result.Title = "Get successfully";

        return result;
    }


}
