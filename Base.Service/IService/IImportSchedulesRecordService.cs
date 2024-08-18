using Base.Repository.Entity;
using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IImportSchedulesRecordService
{
    Task<ServiceResponseVM<IEnumerable<ImportSchedulesRecord>>> GetAllRecord(int startPage, int endPage, int quantity, Guid? userId);
    Task<ServiceResponseVM> RevertRecords(int restoredRecord);
}
