using Base.Repository.Entity;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService
{
    public interface IClassService
    {
        Task<Class> GetClassDetail(int scheduleID);
        Task<ServiceResponseVM<Class>> Add();
    }
}
