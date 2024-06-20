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
    public interface IRoomService
    {
        Task<IEnumerable<Room>> GetAll();
        Task<Room> GetByID(int id);

        Task<ServiceResponseVM<Room>> Create(RoomVM newEntity);
        Task<ServiceResponseVM<Room>> Update(RoomVM updateEntity,int id);
        Task<ServiceResponseVM> Delete(int id);
    }
}
