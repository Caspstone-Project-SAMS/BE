using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Repository
{
    public class ClassRepository : BaseRepository<Class, int>, IClassRepository
    {
        public ClassRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
