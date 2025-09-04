using ADMS.Domain.Entities;
using ADMS.API.Models;

using Mapster;

namespace ADMS.Application.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="RevisionActivity"/>, <see cref="RevisionActivityDto"/>, and <see cref="RevisionActivityMinimalDto"/>.
/// </summary>
public class RevisionActivityProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RevisionActivityProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public RevisionActivityProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<RevisionActivity, RevisionActivityDto>.NewConfig();
        TypeAdapterConfig<RevisionActivity, RevisionActivityMinimalDto>.NewConfig();

        // DTO to Entity mapping
        TypeAdapterConfig<RevisionActivityDto, RevisionActivity>.NewConfig();
    }
}