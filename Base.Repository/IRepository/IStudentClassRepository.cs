using Base.Repository.Common;
using Base.Repository.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.IRepository
{
    public interface IStudentClassRepository : IBaseRepository<StudentClass,StudentClassKey>
    {
    }

    public class StudentClassKey
    {
        public int StudentID { get; set; }
        public int ClassID { get; set; }
    }
}
