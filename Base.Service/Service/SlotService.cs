using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class SlotService : ISlotService
    {
        private readonly IUnitOfWork _unitOfWork;
        public SlotService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    }
}
