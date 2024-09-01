using Base.IService.IService;
using Base.Service.Common;
using Base.Service.IService;
using Base.Service.Service;
using Base.Service.Validation;
using FTask.Service.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.Service;

public static class DependencyInjection
{
    public static IServiceCollection AddService(this IServiceCollection services, IConfiguration configuration)
    {
        #region Services
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IScheduleService, ScheduleService>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<IStudentService, StudentService>();
        services.AddScoped<IClassService, ClassService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<IRoomService, RoomService>();
        services.AddScoped<ISubjectService, SubjectService>();
        services.AddScoped<ISlotService, SlotService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFingerprintService, FingerprintService>();
        services.AddScoped<IModuleActivityService, ModuleActivityService>();
        services.AddScoped<INotificationTypeService, NotificationTypeService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IImportSchedulesRecordService, ImportSchedulesRecordService>();
        services.AddScoped<ISystemService, SystemSevice>();
        services.AddScoped<IScriptService, ScriptService>();
        services.AddScoped<IStudentClassService, StudentClassService>();
        services.AddScoped<ISlotTypeService, SlotTypeService>();
        #endregion

        #region Validation
        services.AddSingleton<ICheckQuantityTaken, CheckQuantityTaken>();
        services.AddSingleton<IValidateGet, ValidateGet>();
        #endregion

        services.AddScoped<IUploadFile, UploadFile>();
        services.AddSingleton(typeof(GoogleSheetsHelper));



        return services;
    }
}
