using Base.Repository.Common;
using Base.Repository.Identity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM.Role;
using Base.Service.ViewModel.ResponseVM;
using FTask.Service.Validation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

public class RoleService : IRoleService
{
    private readonly RoleManager<Role> _roleManager;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;
    private readonly ICurrentUserService _currentUserService;

    public RoleService(IUnitOfWork unitOfWork, IValidateGet validateGet, ICurrentUserService currentUserService, RoleManager<Role> roleManager)
    {
        _roleManager = roleManager;
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
        _currentUserService = currentUserService;
    }

    public async Task<ServiceResponseVM<Role>> Create(Role newRole)
    {
        var existedRoleName = (await _roleManager.FindByNameAsync(newRole.Name)) != null;
        if (existedRoleName)
        {
            return new ServiceResponseVM<Role>
            {
                IsSuccess = false,
                Title = "Create role failed",
                Errors = new string[1] { $"Role name {newRole.Name} is already taken" }
            };
        }

        newRole.CreatedAt = DateTime.UtcNow;
        newRole.CreatedBy = _currentUserService.UserId;

        try
        {
            var result = await _roleManager.CreateAsync(newRole);
            if (result.Succeeded)
            {
                return new ServiceResponseVM<Role>
                {
                    IsSuccess = true,
                    Title = "Create role successfully",
                    Result = newRole
                };
            }
            else
            {
                return new ServiceResponseVM<Role>
                {
                    IsSuccess = false,
                    Title = "Create role failed",
                    Errors = result.Errors.Select(e => e.Description)
                };
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponseVM<Role>
            {
                IsSuccess = false,
                Title = "Create role failed",
                Errors = new string[1] { ex.Message }
            };
        }
        catch (OperationCanceledException ex)
        {
            return new ServiceResponseVM<Role>
            {
                IsSuccess = false,
                Title = "Create role failed",
                Errors = new string[2] { "The operation has been cancelled", ex.Message }
            };
        }
    }

    public async Task<ServiceResponseVM> Delete(Guid id)
    {
        var existedRole = await _unitOfWork.RoleRepository.Get(r => r.Id == id && !r.Deleted).FirstOrDefaultAsync();
        if(existedRole is null)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Delete role failed",
                Errors = new string[1] { "Role not found" }
            };
        }

        existedRole.Deleted = true;
        try
        {
            var result = await _unitOfWork.SaveChangesAsync();
            if (result)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = true,
                    Title = "Delete role successfully"
                };
            }
            else
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete role failed",
                    Errors = new string[1] { "Save changes failed" }
                };
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Delete role failed",
                Errors = new string[1] { ex.Message }
            };
        }
        catch (OperationCanceledException ex)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Delete role failed",
                Errors = new string[2] { "The operation has been cancelled", ex.Message }
            };
        }
    }

    public async Task<IEnumerable<Role>> Get(int startPage, int endPage, int? quantity, string? roleName)
    {
        int quantityResult = 0;
        _validateGet.ValidateGetRequest(ref startPage, ref endPage, quantity, ref quantityResult);
        if(quantityResult == 0)
        {
            throw new ArgumentException("Error when get quantity per page");
        }

        var expressions = new List<Expression>();
        ParameterExpression pe = Expression.Parameter(typeof(Role), "r");
        MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });

        if (containsMethod is null)
        {
            throw new ArgumentNullException("Method Contains can not found from string type");
        }

        expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Role.Deleted)), Expression.Constant(false)));

        if (roleName is not null)
        {
            expressions.Add(Expression.Call(Expression.Property(pe, nameof(Role.Name)), containsMethod, Expression.Constant(roleName)));
        }

        Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
        Expression<Func<Role, bool>> where = Expression.Lambda<Func<Role, bool>>(combined, pe);

        return await _unitOfWork.RoleRepository
            .Get(where)
            .AsNoTracking()
            .Skip((startPage - 1) * quantityResult)
            .Take((endPage - startPage + 1) * quantityResult)
            .ToArrayAsync();
    }

    public async Task<Role?> GetById(Guid id)
    {
        return await _unitOfWork.RoleRepository.Get(r => r.Id == id && !r.Deleted).FirstOrDefaultAsync();
    }

    public async Task<ServiceResponseVM<Role>> Update(Role updateRole, Guid id)
    {
        var existedRole = await _unitOfWork.RoleRepository.Get(r => !r.Deleted && r.Id == id).FirstOrDefaultAsync();
        if(existedRole is null)
        {
            return new ServiceResponseVM<Role>
            {
                IsSuccess = false,
                Title = "Update role failed",
                Errors = new string[1] { "Role not found" }
            };
        }

        if(updateRole.Name != null)
        {
            var checkRoleName = _unitOfWork.RoleRepository.Get(r => r.Id != id && r.Name.Equals(updateRole.Name)).FirstOrDefault() is not null;
            if (checkRoleName)
            {
                return new ServiceResponseVM<Role>
                {
                    IsSuccess = false,
                    Title = "Update role failed",
                    Errors = new string[1] { $"Role name {updateRole.Name} is already taken" }
                };
            }
            existedRole.Name = updateRole.Name;
        }

        var result = await _roleManager.UpdateAsync(existedRole);
        if (result.Succeeded)
        {
            return new ServiceResponseVM<Role>
            {
                IsSuccess = true,
                Title = "Update role successfully",
                Result = existedRole
            };
        }
        else
        {
            return new ServiceResponseVM<Role>
            {
                IsSuccess = false,
                Title = "Update role failed",
                Errors = result.Errors.Select(e => e.Description) 
            };
        }
    }
}
