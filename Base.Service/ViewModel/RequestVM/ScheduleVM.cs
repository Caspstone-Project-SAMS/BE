using Base.Repository.Entity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM
{
    public class ScheduleVM
    {
        public DateOnly Date { get; set; }
        public int SlotNumber { get; set; }
        public string? ClassCode { get; set; }

    }

    public class CreateScheduleVM
    {
        [Required]
        public DateOnly Date { get; set; }
        [Required]
        public int SlotId { get; set; }
        [Required]
        public int ClassId { get; set; }
        public int? RoomId { get; set; }
    }

    public class DeleteSchedulesVM
    {
        [Required]
        public Guid UserID { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public IEnumerable<int> SlotIDs { get; set; } = new List<int>();
    }

    public class UpdateScheduleVM
    {
        public DateOnly? Date { get; set; }
        public int? SlotId { get; set; }
        public int? RoomId { get; set; }
    }
}
