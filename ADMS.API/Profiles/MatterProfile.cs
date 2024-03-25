using AutoMapper;

namespace ADMS.API.Profiles
{
    /// <summary>
    /// Automapper profile
    /// </summary>
    public class MatterProfile : Profile
    {
        /// <summary>
        /// Matter profile constructor
        /// </summary>
        public MatterProfile()
        {
            CreateMap<Entities.Matter, Models.MatterDto>();
            CreateMap<Entities.Matter, Models.MatterForCreationDto>();
            CreateMap<Entities.Matter, Models.MatterForUpdateDto>();
            CreateMap<Entities.Matter, Models.MatterWithDocumentsDto>();
            CreateMap<Entities.Matter, Models.MatterWithoutDocumentsDto>();

            CreateMap<Models.MatterDto, Entities.Matter>();
            CreateMap<Models.MatterForCreationDto, Entities.Matter>();
            CreateMap<Models.MatterForUpdateDto, Entities.Matter>();
            CreateMap<Models.MatterWithDocumentsDto, Entities.Matter>();
            CreateMap<Models.MatterWithoutDocumentsDto, Entities.Matter>();
        }
    }
}
