using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Document Activity Profile for autopmapper
    /// </summary>
    public class DocumentActivityProfile : Profile
    {
        /// <summary>
        /// Document Activity Profile Constructor
        /// </summary>
        public DocumentActivityProfile()
        {
            CreateMap<Entities.DocumentActivity, Models.DocumentActivityDto>();

            CreateMap<Models.DocumentActivityDto, Entities.DocumentActivity>();
        }
    }
}
