using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Matter Document Activity User Profile for autopmapper
    /// </summary>
    public class MatterDocumentActivityUserFromProfile : Profile
    {
        /// <summary>
        /// MatterDocumentActivityUser Profile Constructor
        /// </summary>
        public MatterDocumentActivityUserFromProfile()
        {
            CreateMap<Entities.MatterDocumentActivityUserFrom, Models.MatterDocumentActivityUserFromDto>();
            CreateMap<Entities.MatterDocumentActivityUserFrom, Models.MatterDocumentActivityUserMinimalDto>();

            CreateMap<Models.MatterDocumentActivityUserFromDto, Entities.MatterDocumentActivityUserFrom>();
            CreateMap<Models.MatterDocumentActivityUserFromDto, Models.MatterActivityUserMinimalDto>();
        }
    }
}
