using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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
            u => u.Employee!.Modules,
            u => u.Role
        };
        return await _unitOfWork.UserRepository
            .Get(u => u.EmployeeID == id, includes)
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResponseVM<IEnumerable<User>>> GetAll(int startPage, int endPage, int quantity, string? email, string? phone, string? department, int? roleId)
    {
        var result = new ServiceResponseVM<IEnumerable<User>>()
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
        ParameterExpression pe = Expression.Parameter(typeof(User), "u");
        MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        if (containsMethod is null)
        {
            errors.Add("Method Contains can not found from string type");
            return result;
        }

        expressions.Add(Expression.Equal(Expression.Property(pe, nameof(User.Deleted)), Expression.Constant(false)));
        expressions.Add(Expression.NotEqual(Expression.Property(pe, nameof(User.EmployeeID)), Expression.Constant(null)));

        if (email is not null)
        {
            expressions.Add(Expression.Call(Expression.Property(pe, nameof(User.NormalizedEmail)), containsMethod, Expression.Constant(email.ToUpper())));
        }

        if (phone is not null)
        {
            expressions.Add(Expression.Call(Expression.Property(pe, nameof(User.PhoneNumber)), containsMethod, Expression.Constant(phone)));
        }

        if (department is not null)
        {
            var employeeProperty = Expression.Property(pe, "Employee");
            var departmentProperty = Expression.Property(employeeProperty, "Department");
            expressions.Add(Expression.Equal(departmentProperty, Expression.Constant(department.ToUpper())));
        }

        if (roleId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(User.RoleID)), Expression.Constant(roleId, typeof(int?))));
        }

        Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
        Expression<Func<User, bool>> where = Expression.Lambda<Func<User, bool>>(combined, pe);

        var includes = new Expression<Func<User, object?>>[]
        {
                c => c.Employee,
                c => c.Role
        };

        var classes = await _unitOfWork.UserRepository
            .Get(where, includes)
            .AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult)
            .ToArrayAsync();

        result.IsSuccess = true;
        result.Result = classes;
        result.Title = "Get successfully";

        return result;
    }
}
