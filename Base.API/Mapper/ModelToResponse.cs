using AutoMapper;
using Base.API.Service;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.ViewModel.ResponseVM;

namespace Base.API.Mapper
{
    public class ModelToResponse : Profile
    {
        public ModelToResponse(WebSocketConnectionManager1 webSocketConnectionManager)
        {
            CreateMap<ServiceResponseVM<User>, ServiceResponseVM>();
            CreateMap<ServiceResponseVM<Role>, ServiceResponseVM>();

            CreateMap<User, UserInformationResponseVM>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.GetRole()));

            CreateMap<Role, RoleResponseVM>();
            CreateMap<Semester, SemesterResponse>();
            CreateMap<Room, RoomResponse>();
            CreateMap<Subject, SubjectResponse>();
            CreateMap<Slot, SlotResponse>();

            CreateMap<Schedule, ScheduleResponse>()
                .ForMember(dest => dest.SlotNumber, opt => opt.MapFrom(src => src.Slot!.SlotNumber))
                .ForMember(dest => dest.ClassID, opt => opt.MapFrom(src => src.Class!.ClassID))
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class!.ClassCode))
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src =>
                                                                     src.RoomID != null ? src.Room!.RoomName : src.Class!.Room!.RoomName))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Slot!.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Slot!.Endtime))
                .ForMember(dest => dest.SubjectCode, opt => opt.MapFrom(src => src!.Class!.Subject!.SubjectCode));

            CreateMap<Student, StudentResponse>()
                 .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.User!.DisplayName))
                 .ForMember(dest => dest.StudentID, opt => opt.MapFrom(src => src.StudentID))
                 .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User!.Avatar))
                 .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.User!.Id))
                 .ForMember(dest => dest.AbsencePercentage, opt => opt.MapFrom(src => src.User!.StudentClasses.FirstOrDefault(sc => sc.AbsencePercentage >=0)!.AbsencePercentage))
                 .ForMember(dest => dest.IsAuthenticated, opt => opt.MapFrom(src => src.IsAuthenticated()))
                 .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User!.Email))
                 .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User!.PhoneNumber));

            CreateMap<Student, StudentModuleResponse>()
                 .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.User!.DisplayName))
                 .ForMember(dest => dest.StudentID, opt => opt.MapFrom(src => src.StudentID))
                 .ForMember(dest => dest.FingerprintTemplateData, opt => opt.MapFrom(src => src.FingerprintTemplates.Where(f => f.Status == 1).Select(ft => ft.FingerprintTemplateData)))       
                 .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.User!.Id));

            CreateMap<Class, ClassResponse>()
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room!.IsDeleted ? null : src.Room.RoomName))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Schedules!.FirstOrDefault() != null ? src.Schedules.FirstOrDefault()!.Date : new DateOnly(2000, 0, 0)))
                .ForMember(dest => dest.LecturerName, opt => opt.MapFrom(src => src.Lecturer!.DisplayName))
                .ForMember(dest => dest.SubjectCode, opt => opt.MapFrom(src => src.Subject!.SubjectCode))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject!.SubjectName));

            CreateMap<Attendance, AttendanceResponse>()
                .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.Student!.Student!.StudentCode))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student!.DisplayName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Student!.Email))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Student!.Avatar))
                .ForMember(dest => dest.IsAuthenticated, opt => opt.MapFrom(src => src.Student!.Student!.IsAuthenticated()));


            // For attendance detail
            CreateMap<User, Student_AttendanceResponseVM>()
                .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.Student!.IsDeleted ? null : src.Student.StudentCode));
            CreateMap<Slot, Slot_AttendanceResponseVM>();
            CreateMap<Class, Class_AttendanceResponseVM>();
            CreateMap<Room, Room_AttendanceResponseVM>();
            CreateMap<Schedule, Schedule_AttendanceResponseVM>();
            CreateMap<Attendance, AttendanceResponseVM>();
            CreateMap<Attendance, AttendancesResponseVM>()
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Schedule!.IsDeleted ? new DateOnly(1900, 0 ,0) : src.Schedule.Date))
                .ForMember(dest => dest.Slot, opt => opt.MapFrom(src => src.Schedule!.Slot!.IsDeleted ? null : src.Schedule.Slot))
                .ForMember(dest => dest.Class, opt => opt.MapFrom(src => src.Schedule!.Class!.IsDeleted ? null : src.Schedule!.Class))
                .ForMember(dest => dest.Room, opt => opt.MapFrom(src => src.Schedule!.Room != null ? (src.Schedule.Room.IsDeleted ? null : src.Schedule.Room) : (src.Schedule.Class!.Room!.IsDeleted ? null : src.Schedule.Class.Room)));

            // For class detail
            CreateMap<User, Student_ClassResponseVM>()
                .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.Student!.StudentCode))
                .ForMember(dest => dest.AbsencePercentage, opt => opt.MapFrom(src => src.GetAbsencePercentage()));
            CreateMap<Semester, Semester_ClassResponseVM>();
            CreateMap<Room, Room_ClassResponseVM>();
            CreateMap<Subject, Subject_ClassResponseVM>();
            CreateMap<User, Lecturer_ClassResponseVM>()
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Employee!.Department));
            CreateMap<Schedule, Schedule_ClassResponseVM>();
            CreateMap<Slot, Slot_ClassResponseVM>();
            CreateMap<Class, ClassResponseVM>()
                .ForMember(dest => dest.Students, opt => opt.MapFrom(src => src.Students))
                .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));

            // For schedule detail
            CreateMap<Slot, Slot_ScheduleResponseVM>();
            CreateMap<Class, Class_ScheduleResponseVM>();
            CreateMap<Room, Room_ScheduleResponseVM>();
            CreateMap<Attendance, Attendance_ScheduleResponseVM>();
            CreateMap<User, Student_ScheduleResponseVM>()
                .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.Student!.StudentCode));
            CreateMap<Schedule, ScheduleResponseVM>()
                .ForMember(dest => dest.Attendances, opt => opt.MapFrom(src => src.Attendances));

            // For semester detail
            CreateMap<Class, Class_SemesterResponseVM>();
            CreateMap<Semester, SemesterResponseVM>()
                .ForMember(dest => dest.Classes, opt => opt.MapFrom(src => src.Classes));

            // For slot detail
            CreateMap<Slot, SlotResponseVM>();

            // For student detail
            CreateMap<Class, Class_StudentResponseVM>()
                .ForMember(src => src.AbsencePercentage, opt => opt.MapFrom(src => src.GetAbsencePercentage()));
            CreateMap<FingerprintTemplate, FingerprintTemplate_StudentResponseVM>();
            CreateMap<User, StudentResponseVM>()
                .ForMember(dest => dest.FingerprintTemplates, opt => opt.MapFrom(src => src.Student!.FingerprintTemplates))
                .ForMember(dest => dest.EnrolledClasses, opt => opt.MapFrom(src => src.EnrolledClasses))
                .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.Student!.StudentCode));

            // For employee detail
            CreateMap<Role, Role_EmployeeResponseVM>();
            CreateMap<Class, Class_EmployeeResponseVM>();
            CreateMap<Module, Module_EmployeeResponseVM>();
            CreateMap<User, EmployeeResponseVM>()
                .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src.Employee!.Department))
                .ForMember(dest => dest.ManagedClasses, opt => opt.MapFrom(src => src.ManagedClasses))
                .ForMember(dest => dest.Modules, opt => opt.MapFrom(src => src.Employee!.Modules));

            // For module
            CreateMap<PreparationTask, PreparationTask_ModuleResponseVM>()
                .ForMember(dest => dest.PreparedSchedules, opt => opt.MapFrom(src => src.GetPreparedSchedules()));
            CreateMap<ModuleActivity, ModuleActivity_ModuleResponseVM>()
                .ForMember(dest => dest.Errors, opt => opt.MapFrom(src => src.GetErrors()))
                .ForMember(dest => dest.PreparationTask, opt => opt.MapFrom(src => src.PreparationTask));
            CreateMap<Employee, Employee_ModuleResponseVM>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.User!.Id))
                .ForMember(dest => dest.EmployeeID, opt => opt.MapFrom(src => src.EmployeeID))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.User!.DisplayName))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User!.Avatar))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User!.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.User!.PhoneNumber));
            CreateMap<Module, ModuleResponseVM>()
                .ForMember(dest => dest.ConnectionStatus, opt => 
                    opt.MapFrom(src => webSocketConnectionManager.CheckModuleSocket(src.ModuleID) == true ? 1 : 2))
                .ForMember(dest => dest.ModuleActivities, opt => opt.MapFrom(src => src.ModuleActivities));

            // For notification
            CreateMap<NotificationType, NotificationType_NotificationResponseVM>();
            CreateMap<User, User_NotificationResponseVM>();
            CreateMap<Notification, NotificationResponseVM>()
                .ForMember(dest => dest.NotificationType, opt => opt.MapFrom(src => src.NotificationType))
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User));

            // For notification type
            CreateMap<Notification, Notification_NotificationTypeResponseVM>();
            CreateMap<NotificationType, NotificationTypeResponseVM>()
                .ForMember(dest => dest.Notifications, opt => opt.MapFrom(src => src.Notifications));

            // For module activity
            CreateMap<PreparationTask, PreparationTask_ModuleActivityResponseVM>()
                .ForMember(dest => dest.PreparedSchedules, opt => opt.MapFrom(src => src.GetPreparedSchedules()));
            CreateMap<Module, Module_ModuleActivityResponseVM>();
            CreateMap<ModuleActivity, ModuleActivityResponseVM>()
                .ForMember(dest => dest.Errors, opt => opt.MapFrom(src => src.GetErrors()))
                .ForMember(dest => dest.PreparationTask, opt => opt.MapFrom(src => src.PreparationTask))
                .ForMember(dest => dest.Module, opt => opt.MapFrom(src => src.Module));

            // For import service
            CreateMap<ImportErrorEntity<Schedule>, ImportErrorEntity<Schedule_ImportScheduleServiceResponseVM>>()
                .ForMember(dest => dest.ErrorEntity, opt => opt.MapFrom(src => src.ErrorEntity))
                .ForMember(dest => dest.Errors, opt => opt.MapFrom(src => src.Errors));
            CreateMap<Schedule, Schedule_ImportScheduleServiceResponseVM>()
                .ForMember(dest => dest.SlotNumber, opt => opt.MapFrom(src => src.Slot != null ? src.Slot.SlotNumber : 0))
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassCode : "***"));
            CreateMap<ImportServiceResposneVM<Schedule>, ImportScheduleServiceResponseVM>()
                .ForMember(dest => dest.ImportedEntities, opt => opt.MapFrom(src => src.ImportedEntities))
                .ForMember(dest => dest.ErrorEntities, opt => opt.MapFrom(src => src.ErrorEntities))
                .ForMember(dest => dest.Errors, opt => opt.MapFrom(src => src.Errors));
        }
    }
}
