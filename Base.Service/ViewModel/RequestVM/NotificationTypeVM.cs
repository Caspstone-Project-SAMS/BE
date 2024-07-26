using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM;

public class NotificationTypeVM
{
    [Required]
    public string TypeName { get; set; } = string.Empty;
    public string? TypeDescription { get; set; }
}
