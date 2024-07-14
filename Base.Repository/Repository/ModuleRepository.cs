using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Repository;

internal class ModuleRepository : BaseRepository<Module, int>, IModuleRepository
{
    public ModuleRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
    {
    }
}
