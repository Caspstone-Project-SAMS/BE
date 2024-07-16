using Base.Repository.Entity;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IFingerprintService
{
    Task<ServiceResponseVM<FingerprintTemplate>> CreateNewFinger(Guid studentId, string fingerprintTemplate);
}
