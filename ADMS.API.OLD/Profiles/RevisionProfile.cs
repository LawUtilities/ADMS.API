using ADMS.API.Entities;
using ADMS.API.Models;

using Mapster;

namespace ADMS.API.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="Revision"/> entities and their DTOs.
/// </summary>
public class RevisionProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RevisionProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public RevisionProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<Revision, RevisionDto>.NewConfig();
        TypeAdapterConfig<Revision, RevisionMinimalDto>.NewConfig();
        TypeAdapterConfig<Revision, RevisionForCreationDto>.NewConfig();
        TypeAdapterConfig<Revision, RevisionForUpdateDto>.NewConfig();

        // DTO to Entity mappings
        TypeAdapterConfig<RevisionDto, Revision>.NewConfig();
        TypeAdapterConfig<RevisionForCreationDto, Revision>.NewConfig();
        TypeAdapterConfig<RevisionForUpdateDto, Revision>.NewConfig();

        // DTO to DTO mappings (for convenience and model binding scenarios)
        TypeAdapterConfig<RevisionDto, RevisionMinimalDto>.NewConfig();
        TypeAdapterConfig<RevisionDto, RevisionForCreationDto>.NewConfig();
        TypeAdapterConfig<RevisionDto, RevisionForUpdateDto>.NewConfig();
        TypeAdapterConfig<RevisionForCreationDto, RevisionDto>.NewConfig();
        TypeAdapterConfig<RevisionForUpdateDto, RevisionDto>.NewConfig();
    }
}