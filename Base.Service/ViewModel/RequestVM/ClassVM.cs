using Base.Repository.Entity;
using Base.Repository.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class ClassVM
    {
        public string ClassCode { get; set; } = string.Empty;

        public string SemesterCode { get; set; } = string.Empty;

        public string RoomName { get; set; } = string.Empty;

        public string SubjectCode { get; set; } = string.Empty;

        public Guid LecturerID { get; set; }

        public string CreatedBy { get; set; } = "Undefined";

    }
}
