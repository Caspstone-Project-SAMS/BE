using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM;

public class SlotTypeVM
{
    public string? TypeName { get; set; }
    public string? Description { get; set; }
    public int? Status { get; set; }
    public int? SessionCount { get; set; }
}
