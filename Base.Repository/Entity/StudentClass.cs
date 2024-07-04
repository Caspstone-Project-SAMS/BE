using Base.Repository.Common;
using Base.Repository.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class StudentClass : AuditableEntity
{
    public int AbsencePercentage { get; set; }
    public int ClassID { get; set; }
    public Guid StudentID { get; set; }
}
