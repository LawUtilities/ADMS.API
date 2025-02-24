using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// User Profile for autopmapper
    /// </summary>
    public class UserProfile : Profile
    {
        /// <summary>
        /// User Profile Constructor
        /// </summary>
        public UserProfile()
        {
            CreateMap<Entities.User, Models.UserDto>();
            CreateMap<Entities.User, Models.UserMinimalDto>();

            CreateMap<Models.UserDto, Entities.User>();
        }
    }
}
