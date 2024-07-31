using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.IRepository;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Repository
{
    public class StudentClassRepository : BaseRepository<StudentClass, StudentClassKey>, IStudentClassRepository
    {
        public StudentClassRepository(ApplicationDbContext applicationDbContext) : base(applicationDbContext)
        {
        }

        public override async Task AddAsync(StudentClass entity)
        {
            await base.AddAsync(entity, "StudentClass");
        }

        public override IQueryable<StudentClass> Get(string entityTypeName, Expression<Func<StudentClass, bool>> where)
        {
            return base.Get(entityTypeName, where);
        }

        public async Task<List<StudentClassInfoDto>> GetStudentClassInfoAsync()
        {
            var result = await _applicationDbContext
            .Set<StudentClassInfoDto>()
            .FromSqlRaw("EXEC GetStudentClassInfo")
            .ToListAsync();

            return result;
        }
    }


    
}
