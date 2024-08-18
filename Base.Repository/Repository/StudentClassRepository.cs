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
            var studentClassInfo = from sc in _applicationDbContext.Set<StudentClass>("StudentClass")
                                   join u in _applicationDbContext.Users on sc.StudentID equals u.Id
                                   join c in _applicationDbContext.Classes on sc.ClassID equals c.ClassID
                                   join s in _applicationDbContext.Semesters on c.SemesterID equals s.SemesterID
                                   join st in _applicationDbContext.Students on u.StudentID equals st.StudentID
                                   where sc.IsSendEmail == false && sc.AbsencePercentage > 20
                                   select new StudentClassInfoDto
                                   {
                                       ID = sc.StudentID,
                                       Email = u.Email,
                                       ClassCode = c.ClassCode,
                                       IsSendEmail = sc.IsSendEmail,
                                       AbsencePercentage = sc.AbsencePercentage,
                                       SemesterName = s.SemesterCode,
                                       StudentCode = st.StudentCode,
                                       ClassID = sc.ClassID,                                      
                                   };

            var result = await studentClassInfo.ToListAsync();
            return result;
        }
    }


    
}
