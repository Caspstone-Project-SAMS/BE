using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Repository;

internal class ImportSchedulesRecordRepository : BaseRepository<ImportSchedulesRecord, int>, IImportSchedulesRecordRepository
{
    public ImportSchedulesRecordRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}
