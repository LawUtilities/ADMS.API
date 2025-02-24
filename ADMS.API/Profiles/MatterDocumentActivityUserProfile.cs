using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Matter Document Activity Profile for autopmapper
    /// </summary>
    public class MatterDocumentActivityUserProfile : Profile
    {
        /// <summary>
        /// Matter Document Activity Profile Constructor
        /// </summary>
        public MatterDocumentActivityUserProfile()
        {
            CreateMap<Entities.MatterDocumentActivityUser, Models.MatterDocumentActivityUserDto>();
            CreateMap<Entities.MatterDocumentActivityUserFrom, Models.MatterDocumentActivityUserDto>();
            CreateMap<Entities.MatterDocumentActivityUserTo, Models.MatterDocumentActivityUserDto>();
            CreateMap<Entities.MatterDocumentActivityUser, Models.MatterDocumentActivityUserMinimalDto>();

            CreateMap<Models.MatterDocumentActivityUserDto , Entities.MatterDocumentActivityUser>();
        }
    }
}
