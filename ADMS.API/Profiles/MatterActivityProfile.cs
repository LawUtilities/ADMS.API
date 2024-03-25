using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Matter Activity Profile for autopmapper
    /// </summary>
    public class MatterActivityProfile : Profile
    {
        /// <summary>
        /// Matter Activity Profile Constructor
        /// </summary>
        public MatterActivityProfile()
        {
            CreateMap<Entities.MatterActivity, Models.MatterActivityDto>();

            CreateMap<Models.MatterActivityDto, Entities.MatterActivity>();
        }
    }
}
