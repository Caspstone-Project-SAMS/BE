﻿using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.IRepository;
using Base.Repository.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Repository;

public static class DependencyInjection
{
    public static IServiceCollection AddRepository(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Interceptors
        //services.AddSingleton<UpdateAuditableEntitiesInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(configuration.GetConnectionString("MsSQLConnection") ?? throw new ArgumentNullException("Connection string not found"), b =>
            {
                b.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });
        });

        services.AddTransient<IApplicationDbContext, ApplicationDbContext>();

        services.AddScoped<IUnitOfWork, UnitOfWork>();


        #region Repository
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
        services.AddScoped<IScheduleRepository, ScheduleRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<IClassRepository, ClassRepository>();
        services.AddScoped<IAttendanceRepository, AttendanceRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<ISubjectRepository, SubjectRepository>();
        services.AddScoped<ISlotRepository, SlotRepository>();
        services.AddScoped<IStudentClassRepository, StudentClassRepository>();
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IFingerprintRepository, FingerprintRepository>();
        services.AddScoped<IPreparationTaskRepository, PreparationTaskRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationTypeRepository, NotificationTypeRepository>();
        services.AddScoped<IModuleActivityRepository, ModuleActivityRepository>();
        services.AddScoped<IImportSchedulesRecordRepository, ImportSchedulesRecordRepository>();
        services.AddScoped<ISystemConfigurationRepository, SystemConfigurationRepository>();
        services.AddScoped<IStoredFingerprintDemoRepository, StoredFingerprintDemoRepository>();
        services.AddScoped<ISlotTypeRepository, SlotTypeRepository>();
        #endregion

        return services;
    }
}
