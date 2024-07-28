using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Service.IService;
using Base.Service.ViewModel.RequestVM;
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
            .Count();
    }

    public int GetTotalLecturer()
    {
        return _unitOfWork.UserRepository
            .Get(u => !u.Deleted &&
                u.Role != null &&
                u.Role.NormalizedName.ToUpper() == "LECTURER")
            .Count();
    }

    public int GetTotalSubject()
    {
        return _unitOfWork.SubjectRepository
            .Get(s => !s.IsDeleted)
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
            .Count();
    }
}
