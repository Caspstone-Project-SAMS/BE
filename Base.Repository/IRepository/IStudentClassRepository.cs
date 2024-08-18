using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.IRepository
{
    public interface IStudentClassRepository : IBaseRepository<StudentClass,StudentClassKey>
    {
        Task<List<StudentClassInfoDto>> GetStudentClassInfoAsync();
         
    }

    public class StudentClassKey
    {
        public int StudentID { get; set; }
        public int ClassID { get; set; }
    }

    public class StudentClassInfoDto
    {
        public Guid ID { get; set; }
        public string? Email { get; set; }
        public string? ClassCode { get; set; }
        public int ClassID { get; set; }
        public string StudentCode { get; set; } = string.Empty;
        public bool? IsSendEmail { get; set; }
        public int AbsencePercentage { get; set; }
        public string? SemesterName { get; set; }
    }
}
