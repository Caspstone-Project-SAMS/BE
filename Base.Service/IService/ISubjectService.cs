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
    public interface ISubjectService
    {
        Task<Subject?> GetById(int id);
        Task<IEnumerable<Subject>> Get();
        Task<ServiceResponseVM<Subject>> Create(SubjectVM newEntity);
        Task<ServiceResponseVM> Delete(int id);
        Task<ServiceResponseVM<Subject>> Update(SubjectVM updateEntity, int id);
    }
}
