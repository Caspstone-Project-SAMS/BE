using Base.Repository.Entity;
using Base.Repository.Identity;
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
        Task<IEnumerable<Student>> GetStudentsByClassID(int classID, int startPage, int endPage, int? quantity, Guid? userId);
        Task<ServiceResponseVM> Delete(Guid id);
        Task<ServiceResponseVM<List<StudentVM>>> CreateStudent(List<StudentVM> newEntities);
        Task<ServiceResponseVM<List<StudentClassVM>>> AddStudentToClass(List<StudentClassVM> newEntities, int semesterId);
        Task<User?> GetById(Guid id);
        Task<IEnumerable<Student>> GetStudentsByClassIdv2(int startPage, int endPage, int quantity, Guid? userId, int? classID);
    }
}
