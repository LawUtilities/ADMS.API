using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Document Activity User Profile for autopmapper
    /// </summary>
    public class DocumentActivityUserProfile : Profile
    {
        /// <summary>
        /// Document Activity User Profile Constructor
        /// </summary>
        public DocumentActivityUserProfile()
        {
            CreateMap<Entities.DocumentActivityUser, Models.DocumentActivityUserDto>();

            CreateMap<Models.DocumentActivityUserDto, Entities.DocumentActivityUser>();
        }
    }
}
