using Base.Repository.Entity;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService
{
    public interface ISlotService
    {
        Task<IEnumerable<Slot>> Get();
        Task<ServiceResponseVM<Slot>> Create(SlotVM newEntity);
        Task<ServiceResponseVM> Delete(int id);
        Task<ServiceResponseVM<Slot>> Update(SlotVM updateEntity, int id);
        Task<Slot?> GetById(int id);
    }
}
