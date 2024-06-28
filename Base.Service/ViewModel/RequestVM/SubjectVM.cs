using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class SubjectVM
    {
        [Required]
        public string SubjectCode { get; set; } = string.Empty;
        [Required]
        public string SubjectName { get; set; } = string.Empty;
        public int SubjectStatus { get; set; }
        public string CreatedBy { get; set; } = "Undefined";
    }
}
