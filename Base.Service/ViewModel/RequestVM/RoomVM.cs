using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class RoomVM
    {
        public string RoomName { get; set; } = string.Empty;
        public string? RoomDescription { get; set; }
        public int RoomStatus { get; set; }
    }
}
