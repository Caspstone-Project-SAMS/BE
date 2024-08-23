using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.Validation;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class SlotService : ISlotService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IValidateGet _validateGet;
        public SlotService(IUnitOfWork unitOfWork, IValidateGet validateGet)
        {
            _unitOfWork = unitOfWork;
            _validateGet = validateGet;
        }

        public async Task<ServiceResponseVM<Slot>> Create(SlotVM newEntity)
        {
            var errors = new List<string>();
            if (newEntity.SlotNumber is null)
            {
                errors.Add("Slot number is required");
            }
            if (newEntity.StartTime is null)
            {
                errors.Add("Start time is required");
            }
            if (newEntity.Endtime is null)
            {
                errors.Add("End time is required");
            }
            if(errors.Count > 0)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Create slot failed",
                    Errors = errors.ToArray()
                };
            }

            var existedSlot = await _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted && s.SlotNumber == newEntity.SlotNumber)
                .AsNoTracking()
                .FirstOrDefaultAsync();
            if (existedSlot is not null)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Create slot failed",
                    Errors = new string[1] { "Slot number is already taken" }
                };
            }

            // Validate slot
            if(newEntity.StartTime > newEntity.Endtime)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Create slot failed",
                    Errors = new string[1] { "Start time must be earlier than end time" }
                };
            }
            var slotDuration = _unitOfWork.SystemConfigurationRepository
                .Get(s => true)
                .AsNoTracking()
                .FirstOrDefault()
                ?.SlotDurationInMins ?? 45;
            var difference = newEntity.Endtime!.Value - newEntity.StartTime!.Value;
            if((int)difference.TotalMinutes != slotDuration)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Create slot failed",
                    Errors = new string[2] { $"The total duration of the slot is {(int)difference.TotalMinutes} minutes", $"The total duration of a slot must be {slotDuration} minutes" }
                };
            }

            // Check overlap
            var checkOverlappingSlot = _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted &&
                    ((s.StartTime <= newEntity.StartTime && s.Endtime >= newEntity.StartTime) ||
                    (s.StartTime <= newEntity.Endtime && s.Endtime >= newEntity.Endtime) ||
                    (s.StartTime >= newEntity.StartTime && s.Endtime <= newEntity.Endtime))
                    )
                .AsNoTracking()
                .Any();
            if (checkOverlappingSlot)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Create slot failed",
                    Errors = new string[1] { "Overlap with existing slot" }
                };
            }

            // Identify the order of slot
            var addedSlot = new Slot
            {
                SlotNumber = newEntity.SlotNumber ?? 0,
                Status = newEntity.Status ?? 1,
                StartTime = newEntity.StartTime.Value,
                Endtime =  newEntity.Endtime.Value
            };
            var slots = _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted)
                .ToList();
            slots.Add(addedSlot);
            var orderedSlot = slots.OrderBy(s => s.StartTime).ToList();
            int order = 1;
            foreach(var slot in orderedSlot)
            {
                slot.Order = order++;
            }

            try
            {
                await _unitOfWork.SlotRepository.AddAsync(addedSlot);
                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponseVM<Slot>
                    {
                        IsSuccess = true,
                        Title = "Create slot successfully",
                        Result = addedSlot
                    };
                }
                else
                {
                    return new ServiceResponseVM<Slot>
                    {
                        IsSuccess = false,
                        Title = "Create slot failed",
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Create slot failed",
                    Errors = new string[1] { ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Create slot failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }

        public async Task<ServiceResponseVM> Delete(int id)
        {
            var existedSlot = _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted && s.SlotID == id)
                .FirstOrDefault();
            if(existedSlot is null)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete slot failed",
                    Errors = new string[1] { "Slot not found" }
                };
            }

            var checkSlotIsAlreadyInUse = _unitOfWork.ScheduleRepository
                .Get(s => !s.IsDeleted && s.SlotID == id)
                .AsNoTracking()
                .Count() > 0;
            if (checkSlotIsAlreadyInUse)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete slot failes",
                    Errors = new string[2] { "Cannot delete this slot", "Slot is already in use" }
                };
            }

            existedSlot.IsDeleted = true;

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();
                if (result)
                {
                    return new ServiceResponseVM
                    {
                        IsSuccess = true,
                        Title = "Delete slot successfully"
                    };
                }

                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete slot failed",
                    Errors = new string[1] { "Error when saving changes" }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = false,
                    Title = "Delete slot failed",
                    Errors = new string[2] { "Error when saving changes", ex.Message }
                };
            }
        }

        public async Task<IEnumerable<Slot>> Get()
        {
            return await _unitOfWork.SlotRepository.Get(s => s.IsDeleted != true).ToArrayAsync();
        }

        public async Task<ServiceResponseVM<Slot>> Update(SlotVM updateEntity, int id)
        {
            var existedSlot = await _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted && s.SlotID == id)
                .FirstOrDefaultAsync();
            if (existedSlot is null)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Update slot failed",
                    Errors = new string[1] { "Slot not found" }
                };
            }

            var copySlot = (Slot)existedSlot.Clone();

            if(updateEntity.SlotNumber is not null)
            {
                var checkSlotNumber = _unitOfWork.SlotRepository
                    .Get(s => !s.IsDeleted && s.SlotID != id && s.SlotNumber == updateEntity.SlotNumber)
                    .Any();
                if (checkSlotNumber)
                {
                    return new ServiceResponseVM<Slot>
                    {
                        IsSuccess = false,
                        Title = "Update slot failed",
                        Errors = new string[1] { "Slot number is already taken" }
                    };
                }
                existedSlot.SlotNumber = updateEntity.SlotNumber.Value;
            }

            existedSlot.StartTime = updateEntity.StartTime is null ? existedSlot.StartTime : updateEntity.StartTime.Value;
            existedSlot.Endtime = updateEntity.Endtime is null ? existedSlot.Endtime : updateEntity.Endtime.Value;
            existedSlot.Status = updateEntity.Status is null ? existedSlot.Status : updateEntity.Status ?? 1;

            // Validate slot
            if (existedSlot.StartTime > existedSlot.Endtime)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Update slot failed",
                    Errors = new string[1] { "Start time must be earlier than end time" }
                };
            }
            var slotDuration = _unitOfWork.SystemConfigurationRepository
                .Get(s => true)
                .AsNoTracking()
                .FirstOrDefault()
                ?.SlotDurationInMins ?? 45;
            var difference = existedSlot.Endtime - existedSlot.StartTime;
            if ((int)difference.TotalMinutes != slotDuration)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Update slot failed",
                    Errors = new string[2] { $"The total duration of the slot is {(int)difference.TotalMinutes} minutes", $"The total duration of a slot must be {slotDuration} minutes" }
                };
            }

            // Check overlap slot
            var checkOverlappingSlot = _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted && s.SlotID != existedSlot.SlotID &&
                    ((s.StartTime <= existedSlot.StartTime && s.Endtime >= existedSlot.StartTime) ||
                    (s.StartTime <= existedSlot.Endtime && s.Endtime >= existedSlot.Endtime) ||
                    (s.StartTime >= existedSlot.StartTime && s.Endtime <= existedSlot.Endtime))
                    )
                .AsNoTracking()
                .Any();
            if (checkOverlappingSlot)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Update slot failed",
                    Errors = new string[1] { "Overlap with existing slot" }
                };
            }

            if(copySlot.SlotNumber == existedSlot.SlotNumber && copySlot.Status == existedSlot.Status &&
               copySlot.StartTime == existedSlot.StartTime && copySlot.Endtime == existedSlot.Endtime)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = true,
                    Title = "Update slot successfully",
                    Result = existedSlot
                };
            }

            // Identify the order of slot
            var slots = _unitOfWork.SlotRepository
                .Get(s => !s.IsDeleted && s.SlotID != existedSlot.SlotID)
                .ToList();
            slots.Add(existedSlot);
            var orderedSlot = slots.OrderBy(s => s.StartTime).ToList();
            int order = 1;
            foreach (var slot in orderedSlot)
            {
                slot.Order = order++;
            }

            try
            {
                var result = await _unitOfWork.SaveChangesAsync();

                if (result)
                {
                    return new ServiceResponseVM<Slot>
                    {
                        IsSuccess = true,
                        Title = "Update slot successfully",
                        Result = existedSlot
                    };
                }
                else
                {
                    return new ServiceResponseVM<Slot>
                    {
                        IsSuccess = false,
                        Title = "Update slot failed",
                        Errors = new string[1] { "Errors when saving changes" }
                    };
                }
            }
            catch (DbUpdateException ex)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Update slot failed",
                    Errors = new string[2] { "Errors when saving changes", ex.Message }
                };
            }
            catch (OperationCanceledException ex)
            {
                return new ServiceResponseVM<Slot>
                {
                    IsSuccess = false,
                    Title = "Update slot failed",
                    Errors = new string[2] { "The operation has been cancelled", ex.Message }
                };
            }
        }

        public async Task<Slot?> GetById(int id)
        {
            var includes = new Expression<Func<Slot, object?>>[]
            {
            };
            return await _unitOfWork.SlotRepository
                .Get(s => s.SlotID == id, includes)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }

        public async Task<ServiceResponseVM<IEnumerable<Slot>>> GetAllSlots(
            int startPage, 
            int endPage, 
            int quantity, 
            int? slotNumber, 
            int? status, 
            int? order)
        {
            var result = new ServiceResponseVM<IEnumerable<Slot>>()
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
            ParameterExpression pe = Expression.Parameter(typeof(Slot), "s");

            if(slotNumber is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Slot.SlotNumber)), Expression.Constant(slotNumber)));
            }

            if (status is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Slot.Status)), Expression.Constant(status)));
            }

            if(order is not null)
            {
                expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Slot.Order)), Expression.Constant(order)));
            }

            Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
            Expression<Func<Slot, bool>> where = Expression.Lambda<Func<Slot, bool>>(combined, pe);

            var slots = await _unitOfWork.SlotRepository
                .Get(where)
                .AsNoTracking()
                .Skip((startPage - 1) * quantityResult)
                .Take((endPage - startPage + 1) * quantityResult)
                .ToArrayAsync();

            result.IsSuccess = true;
            result.Result = slots;
            result.Title = "Get successfully";

            return result;
        }
    }
}
