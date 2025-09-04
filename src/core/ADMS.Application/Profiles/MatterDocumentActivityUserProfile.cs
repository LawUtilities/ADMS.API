using ADMS.API.Entities;
using ADMS.API.Models;

using Mapster;

namespace ADMS.Application.Profiles;

/// <summary>
///     AutoMapper profile for mapping between matter document activity user entities and their DTOs.
/// </summary>
public class MatterDocumentActivityUserProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MatterDocumentActivityUserProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public MatterDocumentActivityUserProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<MatterDocumentActivityUser, MatterDocumentActivityUserDto>.NewConfig();
        TypeAdapterConfig<MatterDocumentActivityUserFrom, MatterDocumentActivityUserDto>.NewConfig();
        TypeAdapterConfig<MatterDocumentActivityUserTo, MatterDocumentActivityUserDto>.NewConfig();
        TypeAdapterConfig<MatterDocumentActivityUser, MatterDocumentActivityUserMinimalDto>.NewConfig();

        // DTO to Entity mapping
        TypeAdapterConfig<MatterDocumentActivityUserDto, MatterDocumentActivityUser>.NewConfig();
    }
}