using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Repository
{
    public class AttendanceRepository : BaseRepository<Attendance, int>, IAttendanceRepository
    {
        public AttendanceRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }      
    } 
}
