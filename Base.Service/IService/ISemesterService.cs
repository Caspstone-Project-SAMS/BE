using Base.Repository.Entity;
using Base.Service.ViewModel.RequestVM;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.IService.IService
{
    public interface ISemesterService
    {
        Task<IEnumerable<Semester>> GetSemester();

        Task<ServiceResponseVM<Semester>> Create(SemesterVM newEntity);
        Task<ServiceResponseVM> Delete(int id);
        Task<ServiceResponseVM<Semester>> Update(SemesterVM updateSemester, int id);
    }
}
