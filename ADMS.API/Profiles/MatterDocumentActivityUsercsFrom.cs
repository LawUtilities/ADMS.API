using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Matter Document Activity User Profile for autopmapper
    /// </summary>
    public class MatterDocumentActivityUserFromProfile : Profile
    {
        /// <summary>
        /// MatterDocumentActivityUserFrom Profile Constructor
        /// </summary>
        public MatterDocumentActivityUserFromProfile()
        {
            CreateMap<Entities.MatterDocumentActivityUserFrom, Models.MatterDocumentActivityUserFromDto>();

            CreateMap<Models.MatterDocumentActivityUserFromDto, Entities.MatterDocumentActivityUserFrom>();
        }
    }
}
