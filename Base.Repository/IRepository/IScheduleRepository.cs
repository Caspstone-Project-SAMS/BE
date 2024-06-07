using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.IRepository
{
    public interface IScheduleRepository : IBaseRepository<Schedule,int>
    {
    }
}
