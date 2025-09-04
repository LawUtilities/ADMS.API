using ADMS.Domain.Entities;
using ADMS.Application.DTOs;

using Mapster;

namespace ADMS.Application.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="RevisionActivityUser"/>, <see cref="RevisionActivityUserDto"/>, and <see cref="RevisionActivityUserMinimalDto"/>.
/// </summary>
public class RevisionActivityUserProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RevisionActivityUserProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public RevisionActivityUserProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<RevisionActivityUser, RevisionActivityUserDto>.NewConfig();
        TypeAdapterConfig<RevisionActivityUser, RevisionActivityUserMinimalDto>.NewConfig();

        // DTO to Entity mapping
        TypeAdapterConfig<RevisionActivityUserDto, RevisionActivityUser>.NewConfig();
    }
}