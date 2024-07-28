using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.IService;

public interface IDashboardService
{
    int GetTotalStudents();
    public int GetTotalLecturer();
    public int GetTotalSubject();
    public int GetTotalClass(int? classStatus, int? semesterId, int? roomId, int? subjectId, Guid? lecturerId);
}
