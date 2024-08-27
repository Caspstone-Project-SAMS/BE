using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class SlotVM
    {
        public int? SlotNumber { get; set; }
        public int? Status { get; set; }
        public TimeOnly? StartTime { get; set; }
        public TimeOnly? Endtime { get; set; }
    }

    public class CreateSlotVM
    {
        [Required]
        public int SlotNumber { get; set; }
        public int Status { get; set; } = 1;
        [Required]
        public TimeOnly StartTime { get; set; }
        [Required]
        public TimeOnly Endtime { get; set; }
        [Required]
        public int SlotTypeId { get; set; }
    }
}
