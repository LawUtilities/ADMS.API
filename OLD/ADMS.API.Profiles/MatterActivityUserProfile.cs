using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Matter Activity User Profile for autopmapper
    /// </summary>
    public class MatterActivityUserProfile : Profile
    {
        /// <summary>
        /// Matter Activity User Profile Constructor
        /// </summary>
        public MatterActivityUserProfile()
        {
            CreateMap<Entities.MatterActivityUser, Models.MatterActivityUserDto>();
            CreateMap<Entities.MatterActivityUser, Models.MatterActivityUserMinimalDto>();

            CreateMap<Models.MatterActivityUserDto, Entities.MatterActivityUser>();
        }
    }
}
