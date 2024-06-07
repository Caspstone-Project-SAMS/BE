using Base.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService
{
    public interface IScheduleService
    {
        Task<IEnumerable<Schedule>> GetSchedules(int startPage,int endPage,Guid lecturerId,int quantity, int semesterId,DateTime? startDate, DateTime? endDate);
    }
}
