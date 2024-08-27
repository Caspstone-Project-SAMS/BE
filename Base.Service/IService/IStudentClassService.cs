using Base.Service.ViewModel.ResponseVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IStudentClassService
{
    Task<ServiceResponseVM> DeleteStudentsFromClass(int classId, IEnumerable<Guid> studentIds);
}
