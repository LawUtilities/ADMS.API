using ADMS.API.Entities;
using ADMS.API.Models;

using Mapster;

namespace ADMS.Application.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="MatterActivityUser"/>, <see cref="MatterActivityUserDto"/>, and <see cref="MatterActivityUserMinimalDto"/>.
/// </summary>
public class MatterActivityUserProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MatterActivityUserProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public MatterActivityUserProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<MatterActivityUser, MatterActivityUserDto>.NewConfig();
        TypeAdapterConfig<MatterActivityUser, MatterActivityUserMinimalDto>.NewConfig();

        // DTO to Entity mapping
        TypeAdapterConfig<MatterActivityUserDto, MatterActivityUser>.NewConfig();
    }
}