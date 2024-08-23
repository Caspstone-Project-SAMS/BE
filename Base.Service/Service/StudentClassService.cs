using Base.Repository.Common;
using Base.Service.IService;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class StudentClassService : IStudentClassService
{
    private readonly IUnitOfWork _unitOfWork;
    public StudentClassService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResponseVM> DeleteStudentsFromClass(int classId, IEnumerable<Guid> studentIds)
    {
        var existedClass = _unitOfWork.ClassRepository
            .Get(c => !c.IsDeleted && c.ClassID == classId,
            new System.Linq.Expressions.Expression<Func<Repository.Entity.Class, object?>>[]
            {
                c => c.StudentClasses,
                c => c.Schedules
            })
            .FirstOrDefault();
        
        if(existedClass is null)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Remove students from class failed",
                Errors = new string[1] { "Class not found" }
            };
        }

        if(existedClass.Schedules.Count() > 0)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Remove students from class failed",
                Errors = new string[2] { "Cannot remove students from this class", "Remove all schedules of the class first" }
            };
        }

        var remainedStudentClass = existedClass.StudentClasses.Where(s => !studentIds.Contains(s.StudentID)).ToList();

        if(remainedStudentClass.Count() == existedClass.StudentClasses.Count())
        {
            return new ServiceResponseVM
            {
                IsSuccess = true,
                Title = "Remove students from class successfully"
            };
        }

        existedClass.StudentClasses = remainedStudentClass;

        try
        {
            var result = await _unitOfWork.SaveChangesAsync();
            if (result)
            {
                return new ServiceResponseVM
                {
                    IsSuccess = true,
                    Title = "Remove students from class successfully"
                };
            }

            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Remove students from class failed",
                Errors = new string[1] { "Error when saving changes" }
            };
        }
        catch (Exception ex)
        {
            return new ServiceResponseVM
            {
                IsSuccess = false,
                Title = "Remove students from class failed",
                Errors = new string[2] { "Error when saving changes", ex.Message }
            };
        }
    }
}
