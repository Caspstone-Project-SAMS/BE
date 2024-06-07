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

            CreateMap<Schedule, ScheduleResponse>()
                .ForMember(dest => dest.SlotNumber, opt => opt.MapFrom(src => src.Slot!.SlotNumber))
                .ForMember(dest => dest.ClassCode, opt => opt.MapFrom(src => src.Class!.ClassCode))
                .ForMember(dest => dest.RoomName, opt => opt.MapFrom(src => src.Class!.Room!.RoomName))
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.Slot!.StartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.Slot!.Endtime))
                .ForMember(dest => dest.SubjectCode, opt => opt.MapFrom(src => src!.Class!.Subject!.SubjectCode));
        }
    }
}
