using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.IService;
using Base.Service.ViewModel.ResponseVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service
{
    public class ClassService : IClassService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ClassService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Task<ServiceResponseVM<Class>> Add()
        {
            throw new NotImplementedException();
        }

        public async Task<Class> GetClassDetail(int scheduleID)
        {
            var includes = new Expression<Func<Class, object?>>[]
        {
            c => c.Lecturer,
            c => c.Subject,
            c => c.Schedules,
            c => c.Room,
        };
            var classDetail = await _unitOfWork.ClassRepository.Get(cl => cl.Schedules.Any(c => c.ScheduleID == scheduleID),includes: includes).SingleOrDefaultAsync();
            if(classDetail == null)
            {
                throw new Exception($"Not Found Class With ScheduleID = {scheduleID}");
            }
            return classDetail;
        }
    }
}
