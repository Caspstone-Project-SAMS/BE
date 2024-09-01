using Base.Repository.Entity;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface ISlotTypeService
{
    Task<ServiceResponseVM<IEnumerable<SlotType>>> GetAll(
        int startPage,
        int endPage,
        int quantity,
        string? typeName,
        string? description,
        int? status,
        int? sessionCount);
    Task<SlotType?> GetById(int id);
    Task<ServiceResponseVM<SlotType>> CreateSlotType(SlotTypeVM resource);
    Task<ServiceResponseVM<SlotType>> UpdateSlotType(int slotTypeId, SlotTypeVM resource);
    Task<ServiceResponseVM> DeleteSlotType(int slotTypeId);
}
