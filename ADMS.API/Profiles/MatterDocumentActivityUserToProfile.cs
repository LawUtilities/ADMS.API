using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Matter Document Activity User Profile for autopmapper
    /// </summary>
    public class MatterDocumentActivityUserToProfile : Profile
    {
        /// <summary>
        /// MatterDocumentActivityUser Profile Constructor
        /// </summary>
        public MatterDocumentActivityUserToProfile()
        {
            CreateMap<Entities.MatterDocumentActivityUserTo, Models.MatterDocumentActivityUserToDto>();
            CreateMap<Entities.MatterDocumentActivityUserTo, Models.MatterDocumentActivityUserMinimalDto>();
            CreateMap<Entities.MatterDocumentActivityUserTo, Entities.MatterDocumentActivityUser>();

            CreateMap<Models.MatterDocumentActivityUserToDto, Models.MatterDocumentActivityUserMinimalDto>();
        }
    }
}
