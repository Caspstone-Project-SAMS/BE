using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Entity;

public class ModuleActivity
{
    [Key]
    public int ModuleActivityId { get; set; }

    public string Title { get; set; } = "Activity";
    public string Description { get; set; } = string.Empty;

    public Guid? UserId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsSuccess { get; set; }
    public string? Errors { get; set; } = string.Empty;

    // Activity about schedule preparation
    public int? PreparationTaskID { get; set; }
    public PreparationTask? PreparationTask { get; set; }

    public int ModuleID { get; set; }
    public Module? Module { get; set; }

    public IEnumerable<string> GetErrors()
    {
        return this.Errors?.Split(";") ?? new string[0];
    }

    public string? GetActivityDate()
    {
        return this.StartTime.ToString("yyyy-MM-dd");
    }
}

public class PreparationTask
{
    [Key]
    public int PreparationTaskID { get; set; }
    public float Progress { get; set; }
    public int? PreparedScheduleId { get; set; }
    public string? PreparedSchedules { get; set; }
    public DateOnly? PreparedDate { get; set; }

    public int TotalFingers { get; set; }
    public int UploadedFingers { get; set; }

    public ModuleActivity? ModuleActivity { get; set; }

    public IEnumerable<int> GetPreparedSchedules()
    {
        var scheduleIds = new List<int>();
        var preparedSchedules = this.PreparedSchedules?.Split(";");
        if (preparedSchedules is not null && preparedSchedules.Any())
        {
            foreach (var item in preparedSchedules)
            {
                int scheduleId = 0;
                if (int.TryParse(item, out scheduleId))
                {
                    if (scheduleId > 0)
                    {
                        scheduleIds.Add(scheduleId);
                    }
                }
            }
        }
        return scheduleIds;
    }
}
