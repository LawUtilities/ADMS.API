using ADMS.API.Entities;
using ADMS.API.Models;

using Mapster;

namespace ADMS.Application.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="Matter"/> entities and their DTOs.
/// </summary>
public class MatterProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MatterProfile"/> class and configures entity-to-DTO and DTO-to-entity mappings.
    /// </summary>
    public MatterProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<Matter, MatterDto>.NewConfig();
        TypeAdapterConfig<Matter, MatterMinimalDto>.NewConfig();
        TypeAdapterConfig<Matter, MatterForCreationDto>.NewConfig();
        TypeAdapterConfig<Matter, MatterForUpdateDto>.NewConfig();
        TypeAdapterConfig<Matter, MatterWithDocumentsDto>.NewConfig();
        TypeAdapterConfig<Matter, MatterWithoutDocumentsDto>.NewConfig();

        // DTO to Entity mappings
        TypeAdapterConfig<MatterDto, Matter>.NewConfig();
        TypeAdapterConfig<MatterForCreationDto, Matter>.NewConfig();
        TypeAdapterConfig<MatterForUpdateDto, Matter>.NewConfig();
        TypeAdapterConfig<MatterWithDocumentsDto, Matter>.NewConfig();
        TypeAdapterConfig<MatterWithoutDocumentsDto, Matter>.NewConfig();

        // DTO to DTO mappings
        TypeAdapterConfig<MatterForCreationDto, MatterDto>.NewConfig();
    }
}