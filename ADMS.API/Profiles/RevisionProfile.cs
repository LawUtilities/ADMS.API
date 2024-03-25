using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Revision Profile fpr autopmapper
    /// </summary>
    public class RevisionProfile : Profile
    {
        /// <summary>
        /// Revision Profile Constructor
        /// </summary>
        public RevisionProfile()
        {
            CreateMap<Entities.Revision, Models.RevisionDto>();
            CreateMap<Entities.Revision, Models.RevisionForCreationDto>();
            CreateMap<Entities.Revision, Models.RevisionForUpdateDto>();

            CreateMap<Models.RevisionDto, Entities.Revision>();
            CreateMap<Models.RevisionForCreationDto, Entities.Revision>();
            CreateMap<Models.RevisionForUpdateDto, Entities.Revision>();

            CreateMap<Models.RevisionDto, Models.RevisionForCreationDto>();
            CreateMap<Models.RevisionDto, Models.RevisionForUpdateDto>();
            CreateMap<Models.RevisionForCreationDto, Models.RevisionDto>();
            CreateMap<Models.RevisionForUpdateDto, Models.RevisionDto>();
        }
    }
}
