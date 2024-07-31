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
        public Task<ServiceResponseVM<Slot>> Create(SlotVM newEntity)
        {
            throw new NotImplementedException();
        }

        public Task<ServiceResponseVM> Delete(int id)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Slot>> Get()
        {
            return await _unitOfWork.SlotRepository.Get(s => s.IsDeleted != true).ToArrayAsync();
        }

        public Task<ServiceResponseVM<Slot>> Update(SlotVM updateEntity, int id)
        {
            throw new NotImplementedException();
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
