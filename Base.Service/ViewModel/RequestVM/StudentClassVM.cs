using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class StudentClassVM
    {
        [Required]
        public string? StudentCode { get; set; }

        [Required]
        public string? ClassCode { get; set; }
    }
}
