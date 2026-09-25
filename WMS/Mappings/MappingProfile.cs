using AutoMapper;
using WMS.Models;
using WMS.Models.Response.User;
namespace WMS.Mappings
{
    public class MappingProfile: Profile
    {
        public MappingProfile()
        {
            CreateMap<User, UserResponseModal>()
                .ForMember(
                dest => dest.RoleLevel,
                opt => opt.MapFrom(src => src.RoleLevelNavigation.RoleLevel)
                );
            
        }
    }
}
