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
    public interface IClassService
    { 
        Task<ServiceResponseVM<Class>> Create(ClassVM newEntity);
        Task<Class?> GetById(int classId);
        Task<ServiceResponseVM<IEnumerable<Class>>> GetAllClasses(int startPage, int endPage, int quantity, int? semesterId, string? classCode, int? classStatus, int? roomID, int? subjectID, Guid? lecturerId, Guid? studentId, int? scheduleId);
        Task<IEnumerable<string>> GetAllClassCodes(int? semesterId, Guid? userId);
        Task<ServiceResponseVM<Class>> UpdateClass(int classId, UpdateClassVM resource);
        Task<ServiceResponseVM> DeleteClassById(int classId);
    }
}
