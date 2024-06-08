using Base.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.IService.IService
{
    public interface ISemesterService
    {
        Task<IEnumerable<Semester>> GetSemester();
    }
}
