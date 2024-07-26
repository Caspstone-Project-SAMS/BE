using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM;

public class NotificationVM
{
    [Required]
    public string Title { get; set; } = string.Empty;
    [Required]
    public string Description { get; set; } = string.Empty;
    public bool Read { get; set; } = false;
    [Required]
    public Guid UserID { get; set; }
    [Required]
    public int NotificationTypeID { get; set; }
}
