using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Repository;

internal class SlotTypeRepository : BaseRepository<SlotType, int>, ISlotTypeRepository
{
    public SlotTypeRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
    {
        
    }
}
