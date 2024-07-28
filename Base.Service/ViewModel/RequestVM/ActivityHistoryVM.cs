using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.ViewModel.RequestVM;

public class ActivityHistoryVM
{
    [Required]
    public string Title { get; set; } = "Activity";
    public string Description { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    [Required]
    public DateTime StartTime { get; set; }
    [Required]
    public DateTime EndTime { get; set; }
    [Required]
    public bool IsSuccess { get; set; } = false;
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
    [Required]
    public int ModuleID { get; set; }
    public PreparationTaskVM? PreparationTaskVM { get; set; }
}

public class PreparationTaskVM
{
    [Required]
    public float Progress { get; set; } = 0;
    public int? PreparedScheduleId { get; set; }
    public IEnumerable<int> PreparedScheduleIds { get; set; } = Enumerable.Empty<int>();
    public DateOnly? PreparedDate { get; set; }
}
