using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Revision Activity Profile for autopmapper
    /// </summary>
    public class RevisionActivityProfile : Profile
    {
        /// <summary>
        /// Revision Activity Profile Constructor
        /// </summary>
        public RevisionActivityProfile()
        {
            CreateMap<Entities.RevisionActivity, Models.RevisionActivityDto>();
            CreateMap<Entities.RevisionActivity, Models.RevisionActivityMinimalDto>();

            CreateMap<Models.RevisionActivityDto, Entities.RevisionActivity>();
        }
    }
}
