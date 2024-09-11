using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service.Service;

internal class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    public DashboardService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public int GetTotalStudents()
    {
        return _unitOfWork.UserRepository
            .Get(u => !u.Deleted && 
                u.Role != null && 
                u.Role.NormalizedName.ToUpper() == "STUDENT")
            .AsNoTracking()
            .Count();
    }

    public int GetTotalAuthenticatedStudents()
    {
        return _unitOfWork.UserRepository
            .Get(u => !u.Deleted &&
                u.Role != null &&
                u.Role.NormalizedName.ToUpper() == "STUDENT" &&
                u.Student != null &&
                u.Student.FingerprintTemplates.Any(f => !f.IsDeleted))
            .AsNoTracking()
            .Count();
    }

    public int GetTotalLecturer()
    {
        return _unitOfWork.UserRepository
            .Get(u => !u.Deleted &&
                u.Role != null &&
                u.Role.NormalizedName.ToUpper() == "LECTURER")
            .AsNoTracking()
            .Count();
    }

    public int GetTotalSubject()
    {
        return _unitOfWork.SubjectRepository
            .Get(s => !s.IsDeleted)
            .AsNoTracking()
            .Count();
    }

    public int GetTotalClass(int? classStatus, int? semesterId, int? roomId, int? subjectId, Guid? lecturerId)
    {
        var expressions = new List<Expression>();
        ParameterExpression pe = Expression.Parameter(typeof(Class), "c");

        expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.IsDeleted)), Expression.Constant(false)));

        if (semesterId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.SemesterID)), Expression.Constant(semesterId)));
        }

        if (classStatus is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.ClassStatus)), Expression.Constant(classStatus)));
        }

        if (roomId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.RoomID)), Expression.Constant(roomId)));
        }

        if (subjectId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.SubjectID)), Expression.Constant(subjectId)));
        }

        if (lecturerId is not null)
        {
            expressions.Add(Expression.Equal(Expression.Property(pe, nameof(Class.LecturerID)), Expression.Constant(lecturerId)));
        }

        Expression combined = expressions.Aggregate((accumulate, next) => Expression.AndAlso(accumulate, next));
        Expression<Func<Class, bool>> where = Expression.Lambda<Func<Class, bool>>(combined, pe);

        return _unitOfWork.ClassRepository
            .Get(where)
            .AsNoTracking()
            .Count();
    }

    public int GetTotalModules()
    {
        return _unitOfWork.ModuleRepository
            .Get(m => !m.IsDeleted)
            .AsNoTracking()
            .Count();
    }

    public SchedulesStatistic GetScheduleStatistic(int semesterId)
    {
        var total = _unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Class != null && s.Class.SemesterID == semesterId)
            .AsNoTracking()
            .Count();

        var notYetCount = _unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Class != null &&
                s.Class.SemesterID == semesterId && s.Attended == 1)
            .AsNoTracking()
            .Count();

        var attendedCount = _unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Class != null &&
                s.Class.SemesterID == semesterId && s.Attended == 2)
            .AsNoTracking()
            .Count();

        var absenceCount = _unitOfWork.ScheduleRepository
            .Get(s => !s.IsDeleted && s.Class != null &&
                s.Class.SemesterID == semesterId && s.Attended == 3)
            .AsNoTracking()
            .Count();

        return new SchedulesStatistic
        {
            TotalSchedules = total,
            NotYetCount = notYetCount,
            AttendedCount = attendedCount,
            AbsenceCount = absenceCount
        };
    }

    public IEnumerable<ModuleActivityReport> GetModuleActivityReport(int semesterId)
    {
        var existedSemester = _unitOfWork.SemesterRepository
            .Get(s => !s.IsDeleted && s.SemesterID == semesterId)
            .AsNoTracking()
            .FirstOrDefault();
        if(existedSemester is null)
        {
            return Enumerable.Empty<ModuleActivityReport>();
        }

        var startDate = existedSemester.StartDate;
        var endDate = existedSemester.EndDate;
        var dateBuffer = startDate;

        var statistics = new List<ModuleActivityReport>();
        while (true)
        {
            if(dateBuffer > endDate)
            {
                break;
            }

            var dateString = dateBuffer.ToString("yyyy-MM-dd");
            var activityCount = _unitOfWork.ModuleActivityRepository
                .Get(m => m.StartTime.ToString("yyyy-MM-dd") == dateString)
                .AsNoTracking()
                .Count();
            statistics.Add(new ModuleActivityReport
            {
                Date = dateBuffer,
                TotalActivities = activityCount
            });

            dateBuffer.AddDays(1);
        }

        return statistics;
    }

    public ModuleActivityStatistic GetModuleActivityStatistic(int semesterId)
    {
        var existedSemester = _unitOfWork.SemesterRepository
            .Get(s => !s.IsDeleted && s.SemesterID == semesterId)
            .AsNoTracking()
            .FirstOrDefault();
        if (existedSemester is null)
        {
            return new ModuleActivityStatistic
            {
                SuccessCount = 0,
                FailedCount = 0
            };
        }
        var startDateTime = existedSemester.StartDate.ToDateTime(new TimeOnly(0, 0, 0));
        var endDateTime = existedSemester.EndDate.ToDateTime(new TimeOnly(23, 59, 59));

        var successCount = _unitOfWork.ModuleActivityRepository
            .Get(m => startDateTime <= m.StartTime && m.StartTime <= endDateTime && m.IsSuccess)
            .AsNoTracking()
            .Count();

        var failedCount = _unitOfWork.ModuleActivityRepository
            .Get(m => startDateTime <= m.StartTime && m.StartTime <= endDateTime && !m.IsSuccess)
            .AsNoTracking()
            .Count();

        return new ModuleActivityStatistic
        {
            SuccessCount = successCount,
            FailedCount = failedCount
        };
    }
}

public class SchedulesStatistic
{
    public int TotalSchedules { get; set; }
    public int NotYetCount { get; set; }
    public int AttendedCount { get; set; }
    public int AbsenceCount { get; set; }
}

public class ModuleActivityReport
{
    public DateOnly Date { get; set; }
    public int TotalActivities { get; set; }
}

public class ModuleActivityStatistic
{
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
}