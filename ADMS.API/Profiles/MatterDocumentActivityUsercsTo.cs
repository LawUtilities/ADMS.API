using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Matter Document Activity User Profile for autopmapper
    /// </summary>
    public class MatterDocumentActivityUserToProfile : Profile
    {
        /// <summary>
        /// MatterDocumentActivityUserTo Profile Constructor
        /// </summary>
        public MatterDocumentActivityUserToProfile()
        {
            CreateMap<Entities.MatterDocumentActivityUserTo, Models.MatterDocumentActivityUserToDto>();

            CreateMap<Models.MatterDocumentActivityUserToDto, Entities.MatterDocumentActivityUserTo>();
        }
    }
}
