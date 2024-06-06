using Base.Repository.Common;
using Base.Repository.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class SubstituteTeaching : AuditableEntity
{
    [Key]
    public int SubstituteTeachingID { get; set; }
    public int SubstituteTeachingStatus { get; set; }
    public DateTime? TimeStamp { get; set; }


    // Substitute lecturer
    public Guid SubstituteLecturerID { get; set; }
    public User? SubstituteLecturer { get; set; }


    // Official lecturer
    public Guid OfficialLecturerID { get; set; }
    public User? OfficialLecturer { get; set; }


    public int ScheduleID { get; set; }
    public Schedule? Schedule { get; set; }
}