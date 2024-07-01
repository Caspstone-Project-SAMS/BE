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
        [Required]
        public string StudentCode { get; set; } = string.Empty;
        public string? DisplayName { get; set; } = "Undefined";
        public string? Email { get; set; }
        [Required]
        public string? CreateBy { get; set; }
        //public string normalizedemail { get; set; } = string.empty;
        //public bool emailconfirmed { get; set; } = false;
        //[required]

        //public int CurriculumID { get; set; }
    }
}
