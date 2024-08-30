using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class SlotTypeService : ISlotTypeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IValidateGet _validateGet;
    public SlotTypeService(IUnitOfWork unitOfWork, IValidateGet validateGet)
    {
        _unitOfWork = unitOfWork;
        _validateGet = validateGet;
    }

    public async Task<ServiceResponseVM<IEnumerable<SlotType>>> GetAll(
        int startPage,
        int endPage,
        int quantity,
        string? typeName,
        string? description,
        int? status,
        int? sessionCount)
    {
        var result = new ServiceResponseVM<IEnumerable<SlotType>>()
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

        MethodInfo? containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        if (containsMethod is null)
        {
            errors.Add("Method Contains can not found from string type");
            return result;
        }

        var expressions = new List<Expression>();
        ParameterExpression pe = Expression.Parameter(typeof(SlotType), "s");

        expressions.Add(Expression.Equal(Expression.Property(pe, nameof(SlotType.IsDeleted)), Expression.Constant(false)));

        if (typeName is not null)
        {
            expressions.Add(Expression.Call(Expression.Property(pe, nameof(SlotType.TypeName)), containsMethod, Expression.Constant(typeName)));
        }

        if(description is not null)
        {
            expressions.Add(Expression.Call(Expression.Property(pe, nameof(SlotType.Description)), containsMethod, Expression.Constant(description)));
        }

        if (status is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(SlotType.Status)), Expression.Constant(status)));
        }

        if (sessionCount is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(SlotType.SessionCount)), Expression.Constant(sessionCount)));
        }

        Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
        Expression<Func<SlotType, bool>> where = Expression.Lambda<Func<SlotType, bool>>(combined, pe);

        var slotTypes = await _unitOfWork.SlotTypeRepository
                .Get(where, 
                new Expression<Func<SlotType, object?>>[]
                {
                    s => s.Slots
                })
                .AsNoTracking()
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult)
                .ToArrayAsync();

        result.IsSuccess = true;
        result.Result = slotTypes;
        result.Title = "Get successfully";

        return result;
    }

    public async Task<SlotType?> GetById(int id)
    {
        return await _unitOfWork.SlotTypeRepository
            .Get(s => !s.IsDeleted && s.SlotTypeID == id,
            new Expression<Func<SlotType, object?>>[]
            {
                s => s.Slots
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ServiceResponseVM<SlotType>> CreateSlotType(SlotTypeVM resource)
    {
        var result = new ServiceResponseVM<SlotType>
        {
            IsSuccess = false
        };
        var errors = new List<string>();

        if(resource.TypeName is null || resource.TypeName == string.Empty)
        {
            errors.Add("Type name is required");
        }

        if(resource.SessionCount is null || resource.SessionCount <= 0)
        {
            errors.Add("Number of sessions is required");
        }

        if (errors.Count() > 0)
        {
            result.IsSuccess = false;
            result.Title = "Create slot type failed";
            result.Errors = errors;
            return result;
        }
        errors = new List<string>();

        var checkExistedTypeName = _unitOfWork.SlotTypeRepository
            .Get(s => !s.IsDeleted && s.TypeName == resource.TypeName)
            .FirstOrDefault() is not null;
        if (checkExistedTypeName)
        {
            errors.Add("Type name is already taken");
        }

        var checkExistedSessionCount = _unitOfWork.SlotTypeRepository
            .Get(s => !s.IsDeleted && s.SessionCount == resource.SessionCount)
            .FirstOrDefault() is not null;
        if (checkExistedSessionCount)
        {
            errors.Add($"{resource.SessionCount}-sessions slot type is already existed");
        }

        if (errors.Count() > 0)
        {
            result.IsSuccess = false;
            result.Title = "Create slot type failed";
            result.Errors = errors;
            return result;
        }

        SlotType newSlotType = new SlotType
        {
            TypeName = resource.TypeName!,
            Description = resource.Description ?? string.Empty,
            SessionCount = (int)resource.SessionCount!,
            Status = resource.Status ?? 1
        };

        try
        {
            await _unitOfWork.SlotTypeRepository.AddAsync(newSlotType);

            var saveChangesResult = await _unitOfWork.SaveChangesAsync();

            if (saveChangesResult)
            {
                return new ServiceResponseVM<SlotType>
                {
                    IsSuccess = true,
                    Title = "Create slot type successfully",
                    Result = newSlotType
                };
            }
            else
            {
                return new ServiceResponseVM<SlotType>
                {
                    IsSuccess = false,
                    Title = "Create slot type failed",
                };
            }
        }
        catch (DbUpdateException ex)
        {
            return new ServiceResponseVM<SlotType>
            {
                IsSuccess = false,
                Title = "Create slot type failed",
                Errors = new string[1] { ex.Message }
            };
        }
        catch (OperationCanceledException ex)
        {
            return new ServiceResponseVM<SlotType>
            {
                IsSuccess = false,
                Title = "Create slot type failed",
                Errors = new string[2] { "The operation has been cancelled", ex.Message }
            };
        }
    }

    public async Task<ServiceResponseVM<SlotType>> UpdateSlotType(int slotTypeId, SlotTypeVM resource)
    {
        var result = new ServiceResponseVM<SlotType>
        {
            IsSuccess = false
        };
        var errors = new List<string>();

        var existedSlotType = _unitOfWork.SlotTypeRepository
                .Get(c => !c.IsDeleted && c.SlotTypeID == slotTypeId)
                .FirstOrDefault();

        if (existedSlotType is null)
        {
            result.IsSuccess = false;
            result.Title = "Update slot type failed";
            result.Errors = new string[1] { "Slot type not found" };
            return result;
        }

        if(resource.TypeName is null &&
            resource.Description is null &&
            resource.Status is null &&
            resource.SessionCount is null)
        {
            result.IsSuccess = true;
            result.Title = "Update slot type successfully";
            result.Result = existedSlotType;
            return result;
        }

        var copySlotType = (SlotType)existedSlotType.Clone();

        if(resource.TypeName is not null)
        {
            var checkExistedTypeName = _unitOfWork.SlotTypeRepository
                .Get(s => !s.IsDeleted && s.SlotTypeID != existedSlotType.SlotTypeID && s.TypeName == resource.TypeName)
                .AsNoTracking()
                .FirstOrDefault() is not null;
            if (checkExistedTypeName)
            {
                errors.Add("Type name is already taken");
            }
            else
            {
                existedSlotType.TypeName = resource.TypeName;
            }
        }

        if(resource.SessionCount is not null)
        {
            var checkExistedSessionCount = _unitOfWork.SlotTypeRepository
                .Get(s => !s.IsDeleted && s.SlotTypeID != existedSlotType.SlotTypeID && s.SessionCount == resource.SessionCount)
                .AsNoTracking()
                .FirstOrDefault() is not null;
            if (checkExistedSessionCount)
            {
                errors.Add($"{resource.SessionCount}-sessions slot type is already existed");
            }
            else
            {
                var checkSlotTypeAlreadyInUse = _unitOfWork.SlotRepository
                    .Get(s => !s.IsDeleted && s.SlotTypeId == slotTypeId)
                    .AsNoTracking()
                    .Count() > 0;
                if (checkSlotTypeAlreadyInUse)
                {
                    errors.Add("Slot type is already in use, unable to update session count");
                }
                else
                {
                    existedSlotType.SessionCount = (int)resource.SessionCount;
                }
            }
        }

        if (errors.Count() > 0)
        {
            result.IsSuccess = false;
            result.Errors = errors;
            return result;
        }

        existedSlotType.Description = resource.Description is null ? existedSlotType.Description : resource.Description;
        existedSlotType.Status = resource.Status is null ? existedSlotType.Status : (int)resource.Status;

        if(TwoObjectAreTheSame(copySlotType, existedSlotType))
        {
            result.IsSuccess = true;
            result.Title = "Update slot type successfully";
            result.Result = existedSlotType;
            return result;
        }

        try
        {
            var finalResult = await _unitOfWork.SaveChangesAsync();
            if (finalResult)
            {
                result.IsSuccess = true;
                result.Result = existedSlotType;
                result.Title = "Update class successfully";
                return result;
            }

            result.IsSuccess = false;
            result.Errors = new string[1] { "Error when saving changes" };
            return result;
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.Errors = new string[2] { "Error when saving changes", ex.Message };
            return result;
        }
    }

    public async Task<ServiceResponseVM> DeleteSlotType(int slotTypeId)
    {
        var existedSlotType = _unitOfWork.SlotTypeRepository
            .Get(s => !s.IsDeleted && s.SlotTypeID == slotTypeId)
            .FirstOrDefault();

        if (existedSlotType is null)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Delete slot type failed",
                Errors = new string[1] { "Slot type not found" }
            };
        }

        var checkAlreadyInUseSlotType = _unitOfWork.SlotRepository
            .Get(s => !s.IsDeleted && s.SlotTypeId == slotTypeId)
            .AsNoTracking()
            .Count() > 0;
        var checkAlreadyInUseSlotType2 = _unitOfWork.ClassRepository
            .Get(c => !c.IsDeleted && c.SlotTypeId == slotTypeId)
            .AsNoTracking()
            .Count() > 0;
        if(checkAlreadyInUseSlotType || checkAlreadyInUseSlotType2)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Delete slot type failed",
                Errors = new string[1] { "Slot type is already in use" }
            };
        }

        existedSlotType.IsDeleted = true;

        try
        {
            var result = await _unitOfWork.SaveChangesAsync();
            if (result)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = true,
                    Title = "Delete slot type successfully"
                };
            }

            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Delete slot type failed",
                Errors = new string[1] { "Error when saving changes" }
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Delete slot type failed",
                Errors = new string[2] { "Error when saving changes", ex.Message }
            };
        }
    }

    private bool TwoObjectAreTheSame(SlotType object1, SlotType object2)
    {
        return object1.TypeName == object2.TypeName && object1.Description == object2.Description &&
            object1.Status == object2.Status && object1.SessionCount == object2.SessionCount;
    }
}
