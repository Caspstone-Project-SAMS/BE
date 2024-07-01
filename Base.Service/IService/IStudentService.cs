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
    public interface IStudentService
    {
        Task<IEnumerable<Student>> GetStudents(int startPage,int endPage,int? quantity,Guid? studentID,string? studentCode);
        Task<IEnumerable<Student>> GetStudentsByClassID(int classID);
        Task<ServiceResponseVM<List<Student>>> CreateStudent(List<StudentVM> newEntities);
    }
}
