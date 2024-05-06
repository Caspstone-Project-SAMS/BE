using Base.Repository.Common;
using Base.Repository.Identity;
using Base.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Repository;

internal class RoleRepository : BaseRepository<Role, Guid>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
    {
    }
}
