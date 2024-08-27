using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
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

internal class EmployeeService : IEmployeeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;
    private readonly ICurrentUserService _currentUserService;
    private readonly UserManager<User> _userManager;
    private readonly IMailService _mailService;
    public EmployeeService(IUnitOfWork unitOfWork, IValidateGet validateGet, ICurrentUserService currentUserService,
                           IMailService mailService, UserManager<User> userManager)

    {
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
        _currentUserService = currentUserService;
        _mailService = mailService;
        _userManager = userManager;
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
            .Include(nameof(User.ManagedClasses) + "." + nameof(Class.Room))
            .Include(nameof(User.ManagedClasses) + "." + nameof(Class.Semester))
            .Include(nameof(User.ManagedClasses) + "." + nameof(Class.Subject))
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

    public async Task<ServiceResponseVM<List<EmployeeVM>>> CreateEmployee(List<EmployeeVM> newEntities)
    {
        List<EmployeeVM> createdEmployee = new List<EmployeeVM>();
        List<string> errors = new List<string>();

        foreach (var newEntity in newEntities)
        {
            var existedUserName = await _unitOfWork.UserRepository.Get(st => st.UserName.Equals(newEntity.UserName)).FirstOrDefaultAsync();
            if (existedUserName is not null)
            {
                errors.Add($"User with user name: {newEntity.UserName} is already taken");
                continue;
            }

            var existingUserEmail = await _unitOfWork.UserRepository.Get(u => u.Email == newEntity.Email).FirstOrDefaultAsync();
            if (existingUserEmail != null)
            {
                errors.Add($"User with email: {newEntity.Email} is already exists.");
                continue;
            }

            Employee newEmployee = new Employee
            {
                Department = "",
                CreatedBy = _currentUserService.UserId,
                CreatedAt = ServerDateTime.GetVnDateTime(),
            };

            try
            {
                await _unitOfWork.EmployeeRepository.AddAsync(newEmployee);

                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    var employeeId = newEmployee.EmployeeID;

                    User newUser = new User
                    {
                        UserName = newEntity.UserName,
                        EmployeeID = employeeId,
                        Email = newEntity.Email,
                        DisplayName = newEntity.DisplayName,
                        RoleID = 2,
                        CreatedBy = _currentUserService.UserId,
                        CreatedAt = ServerDateTime.GetVnDateTime(),
                    };
                    var password = GenerateRandomPassword(6);
                    var identityResult = await _userManager.CreateAsync(newUser, password);

                    if (identityResult.Succeeded)
                    {
                        createdEmployee.Add(newEntity);
                        var emailMessage = new Message
                        {
                            To = newEntity.Email,
                            Subject = "Your account has been created",
                            Content = $@"<html>
                                <body>
                                <p>Dear Lecturer,</p>
                                <p>Your account has been created successfully. Here are your login details:</p>
                                <ul>
                                     <li><strong>Username:</strong> {newEntity.UserName}</li>
                                     <li><strong>Password:</strong> {password}</li>
                                </ul>
                                <p>Best regards,<br>SAMS Team</p>
                                </body>
                                </html>"
                        };
                        await _mailService.SendMailAsync(emailMessage);
                    }
                    else
                    {
                        errors.Add($"Failed to create User for Employee {newEntity.UserName}");
                    }
                }
                else
                {
                    errors.Add($"Failed to save Employee with UserName {newEntity.UserName}");
                }
            }
            catch (DbUpdateException ex)
            {
                errors.Add($"DbUpdateException for UserName {newEntity.UserName}: {ex.Message}");
            }
            catch (OperationCanceledException ex)
            {
                errors.Add($"OperationCanceledException for UserName {newEntity.UserName}: {ex.Message}");
            }
        }
            return new ServiceResponseVM<List<EmployeeVM>>
            {
                IsSuccess = createdEmployee.Count > 0,
                Title = createdEmployee.Count > 0 ? "Create Employee Result" : "Create Employee failed",
                Result = createdEmployee,
                Errors = errors.ToArray()
            };
        
    }

    private string GenerateRandomPassword(int length)
    {
        const string validChars = "abcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder result = new StringBuilder();
        Random random = new Random();
        for (int i = 0; i < length; i++)
        {
            result.Append(validChars[random.Next(validChars.Length)]);
        }
        return result.ToString();
    }
}
