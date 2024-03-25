using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Matter Document Activity Profile for autopmapper
    /// </summary>
    public class MatterDocumentActivityProfile : Profile
    {
        /// <summary>
        /// Matter Document Activity Profile Constructor
        /// </summary>
        public MatterDocumentActivityProfile()
        {
            CreateMap<Entities.MatterDocumentActivity, Models.MatterDocumentActivityDto>();

            CreateMap<Models.MatterDocumentActivityDto , Entities.MatterDocumentActivity>();
        }
    }
}
