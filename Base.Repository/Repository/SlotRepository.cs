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
    public class SlotRepository : BaseRepository<Slot, int>, ISlotRepository
    {
        public SlotRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }
    }
}
