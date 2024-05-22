using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class StudentVM
    {
        public Guid StudentID { get; set; }
        [Required]
        public string StudentCode { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        //public string normalizedemail { get; set; } = string.empty;
        //public bool emailconfirmed { get; set; } = false;
        //[required]
        public string Phone { get; set; } = string.Empty;

        //public int CurriculumID { get; set; }
    }
}
