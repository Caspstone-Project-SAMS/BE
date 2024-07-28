using Base.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository.Common;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IRoleRepository RoleRepository { get; }

    IScheduleRepository ScheduleRepository { get; }

    ISemesterRepository SemesterRepository { get; }
    IStudentRepository StudentRepository { get; }

    IClassRepository ClassRepository { get; }
    IAttendanceRepository AttendanceRepository { get; }

    IRoomRepository RoomRepository { get; }

    ISubjectRepository SubjectRepository { get; }

    ISlotRepository SlotRepository { get; }

    IStudentClassRepository StudentClassRepository { get; }
    
    IModuleRepository ModuleRepository { get; }

    IEmployeeRepository EmployeeRepository { get; }

    IFingerprintRepository FingerprintRepository { get; }

    IPreparationTaskRepository PreparationTaskRepository { get; }

    INotificationRepository NotificationRepository { get; }

    INotificationTypeRepository NotificationTypeRepository { get; }

    IModuleActivityRepository ModuleActivityRepository { get; }

    Task<bool> SaveChangesAsync();
    void Dispose();
}

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ApplicationDbContext _applicationDbContext;

    public IUserRepository UserRepository { get; private set; }
    public IRoleRepository RoleRepository { get; private set; }

    public IScheduleRepository ScheduleRepository { get; private set; }

    public ISemesterRepository SemesterRepository { get; private set; }

    public IStudentRepository StudentRepository { get; private set; }

    public IClassRepository ClassRepository { get; private set; }

    public IAttendanceRepository AttendanceRepository { get; private set; }

    public IRoomRepository RoomRepository { get; private set; }

    public ISubjectRepository SubjectRepository { get; private set; }

    public ISlotRepository SlotRepository { get; private set; }

    public IStudentClassRepository StudentClassRepository { get; private set; }

    public IModuleRepository ModuleRepository { get; private set; }

    public IEmployeeRepository EmployeeRepository { get; private set; }

    public IFingerprintRepository FingerprintRepository { get; private set;  }


    public IPreparationTaskRepository PreparationTaskRepository { get; private set; }

    public INotificationRepository NotificationRepository { get; private set; }

    public INotificationTypeRepository NotificationTypeRepository { get; private set; }

    public IModuleActivityRepository ModuleActivityRepository { get; private set; }
    public UnitOfWork(ApplicationDbContext applicationDbContext,
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IScheduleRepository scheduleRepository,
        ISemesterRepository semesterRepository,
        IStudentRepository studentRepository,
        IClassRepository classRepository,
        IAttendanceRepository attendanceRepository,
        IRoomRepository roomRepository,
        ISubjectRepository subjectRepository,
        ISlotRepository slotRepository,
        IStudentClassRepository studentClassRepository,
        IModuleRepository moduleRepository,
        IEmployeeRepository employeeRepository,
        IFingerprintRepository fingerprintRepository,
        IPreparationTaskRepository preparationTaskRepository,
        INotificationRepository notificationRepository,
        INotificationTypeRepository notificationTypeRepository,
        IModuleActivityRepository moduleActivityRepository)
    {
        _applicationDbContext = applicationDbContext;
        UserRepository = userRepository;
        RoleRepository = roleRepository;
        ScheduleRepository = scheduleRepository;
        SemesterRepository = semesterRepository;
        StudentRepository = studentRepository;
        ClassRepository = classRepository;
        AttendanceRepository = attendanceRepository;
        RoomRepository = roomRepository;
        SubjectRepository = subjectRepository;
        SlotRepository = slotRepository;
        StudentClassRepository = studentClassRepository;
        ModuleRepository = moduleRepository;
        EmployeeRepository = employeeRepository;
        FingerprintRepository = fingerprintRepository;
        PreparationTaskRepository = preparationTaskRepository;
        NotificationRepository = notificationRepository;
        NotificationTypeRepository = notificationTypeRepository;
        ModuleActivityRepository = moduleActivityRepository;
    }

    public async Task<bool> SaveChangesAsync()
    {
        return (await _applicationDbContext.SaveChangesAsync() > 0);
    }

    public void Dispose()
    {
        _applicationDbContext.Dispose();
    }
}
