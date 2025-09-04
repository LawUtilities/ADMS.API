using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Automapper profile
    /// </summary>
    public class DocumentProfile : Profile
    {
        /// <summary>
        /// Document profile constructor
        /// </summary>
        public DocumentProfile()
        {
            CreateMap<Entities.Document, Models.DocumentDto>();
            CreateMap<Entities.Document, Models.DocumentMinimalDto>();
            CreateMap<Entities.Document, Models.DocumentFullDto>();
            CreateMap<Entities.Document, Models.DocumentForCreationDto>();
            CreateMap<Entities.Document, Models.DocumentForUpdateDto>();
            CreateMap<Entities.Document, Models.DocumentWithRevisionsDto>();
            CreateMap<Entities.Document, Models.DocumentWithoutRevisionsDto>();

            CreateMap<Models.DocumentDto, Entities.Document>();
            CreateMap<Models.DocumentFullDto, Entities.Document>();
            CreateMap<Models.DocumentForCreationDto, Entities.Document>();
            CreateMap<Models.DocumentForUpdateDto , Entities.Document>();
            CreateMap<Models.DocumentWithRevisionsDto, Entities.Document>();
            CreateMap<Models.DocumentWithoutRevisionsDto, Entities.Document>();
        }
    }
}
