using Base.Service.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IDashboardService
{
    int GetTotalStudents();
    int GetTotalAuthenticatedStudents();
    public int GetTotalLecturer();
    public int GetTotalSubject();
    public int GetTotalClass(int? classStatus, int? semesterId, int? roomId, int? subjectId, Guid? lecturerId);
    int GetTotalModules();
    SchedulesStatistic GetScheduleStatistic(int semesterId);
    IEnumerable<ModuleActivityReport> GetModuleActivityReport(int semesterId);
    ModuleActivityStatistic GetModuleActivityStatistic(int semesterId);
}
