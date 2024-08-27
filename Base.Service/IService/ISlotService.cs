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
        Task<ServiceResponseVM<Slot>> Create(CreateSlotVM newEntity);
        Task<ServiceResponseVM> Delete(int id);
        Task<ServiceResponseVM<Slot>> Update(SlotVM updateEntity, int id);
        Task<Slot?> GetById(int id);
        Task<ServiceResponseVM<IEnumerable<Slot>>> GetAllSlots(int startPage, int endPage, int quantity, int? slotNumber, int? status,  int? order);
    }
}
