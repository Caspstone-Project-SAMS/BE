using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM;

public class ForgetPasswordVM
{
    [Required]
    public string? Token { get; set; }
    [Required]
    [EmailAddress]
    public string? Email { get; set; }
    [Required]
    [MinLength(5)]
    public string? NewPassword { get; set; }
    [Required]
    [MinLength(5)]
    public string? ConfirmPassword { get; set; }
}
