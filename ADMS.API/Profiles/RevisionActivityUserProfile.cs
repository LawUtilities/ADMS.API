using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Revision Activity User Profile for autopmapper
    /// </summary>
    public class RevisionActivityUserProfile : Profile
    {
        /// <summary>
        /// Revision Activity User Profile Constructor
        /// </summary>
        public RevisionActivityUserProfile()
        {
            CreateMap<Entities.RevisionActivityUser, Models.RevisionActivityUserDto>();

            CreateMap<Models.RevisionActivityUserDto, Entities.RevisionActivityUser>();
        }
    }
}
