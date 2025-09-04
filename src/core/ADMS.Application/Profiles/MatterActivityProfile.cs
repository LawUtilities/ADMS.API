using ADMS.Domain.Entities;
using ADMS.Application.DTOs;

using Mapster;

namespace ADMS.Application.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="MatterActivity"/>, <see cref="MatterActivityDto"/>, and <see cref="MatterActivityMinimalDto"/>.
/// </summary>
public class MatterActivityProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MatterActivityProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public MatterActivityProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<MatterActivity, MatterActivityDto>.NewConfig();
        TypeAdapterConfig<MatterActivity, MatterActivityMinimalDto>.NewConfig();

        // DTO to Entity mapping
        TypeAdapterConfig<MatterActivityDto, MatterActivity>.NewConfig();
    }
}