using AutoMapper;
using Base.Repository.Common;
using Base.Repository.Entity;
using Base.Repository.Identity;
using Base.Service.ViewModel.ResponseVM;

namespace Base.API.Mapper
{
    public class ModelToResponse : Profile
    {
        public ModelToResponse()
        {
            CreateMap<ServiceResponseVM<User>, ServiceResponseVM>();
            CreateMap<ServiceResponseVM<Role>, ServiceResponseVM>();

            CreateMap<User, UserInformationResponseVM>()
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.GetRole()));

            CreateMap<Role, RoleResponseVM>();
            CreateMap<Semester, SemesterResponse>();

            CreateMap<Schedule, ScheduleResponse>()
                .ForMember(dest => dest.SlotNumber, opt => opt.MapFrom(src => src.Slot!.SlotNumber))
                .ForMember(dest => dest.ClassID, opt => opt.MapFrom(src => src.Class!.ClassID))
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class!.ClassCode))
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Class!.Room!.RoomName))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Slot!.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Slot!.Endtime))
                .ForMember(dest => dest.SubjectCode, opt => opt.MapFrom(src => src!.Class!.Subject!.SubjectCode));

            CreateMap<Student, StudentResponse>()
                 .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.User!.DisplayName))
                 .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.User!.Avatar))
                 .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.User!.Id))
                 .ForMember(dest => dest.IsAuthenticated, opt => opt.MapFrom(src => src.IsAuthenticated()));

            CreateMap<Student, StudentModuleResponse>()
                 .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.User!.DisplayName))
                 .ForMember(dest => dest.FingerprintTemplateData, opt => opt.MapFrom(src => src.FingerprintTemplates.Select(ft => ft.FingerprintTemplateData)))       
                 .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.User!.Id));

            CreateMap<Class, ClassResponse>()
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Room!.RoomName))
                .ForMember(dest => dest.Date, opt => opt.MapFrom(src => src.Schedules!.First().Date))
                .ForMember(dest => dest.LecturerName, opt => opt.MapFrom(src => src.Lecturer!.DisplayName))
                .ForMember(dest => dest.SubjectCode, opt => opt.MapFrom(src => src.Subject!.SubjectCode))
                .ForMember(dest => dest.SubjectName, opt => opt.MapFrom(src => src.Subject!.SubjectName));

            CreateMap<Attendance, AttendanceResponse>()
                .ForMember(dest => dest.StudentCode, opt => opt.MapFrom(src => src.Student!.Student!.StudentCode))
                .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => src.Student!.DisplayName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Student!.Email))
                .ForMember(dest => dest.Avatar, opt => opt.MapFrom(src => src.Student!.Avatar))
                .ForMember(dest => dest.IsAuthenticated, opt => opt.MapFrom(src => src.Student!.Student!.IsAuthenticated()));


        }
    }
}
