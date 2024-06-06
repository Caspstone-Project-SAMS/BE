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
        }
    }
}
