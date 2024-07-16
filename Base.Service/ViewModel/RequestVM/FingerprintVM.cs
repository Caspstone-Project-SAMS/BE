using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM;

public class FingerprintVM
{
    [Required]
    public string FingerprintTemplate { get; set; } = string.Empty;
    [Required]
    public Guid StudentID { get; set; }
    [Required]
    public int SessionID { get; set; }
}
