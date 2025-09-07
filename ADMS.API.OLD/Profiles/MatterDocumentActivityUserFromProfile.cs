using ADMS.API.Entities;
using ADMS.API.Models;

using Mapster;

namespace ADMS.API.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="MatterDocumentActivityUserFrom"/>, <see cref="MatterDocumentActivityUserFromDto"/>, and <see cref="MatterDocumentActivityUserMinimalDto"/>.
/// </summary>
public class MatterDocumentActivityUserFromProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MatterDocumentActivityUserFromProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public MatterDocumentActivityUserFromProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<MatterDocumentActivityUserFrom, MatterDocumentActivityUserFromDto>.NewConfig();
        TypeAdapterConfig<MatterDocumentActivityUserFrom, MatterDocumentActivityUserMinimalDto>.NewConfig();

        // DTO to Entity mapping
        TypeAdapterConfig<MatterDocumentActivityUserFromDto, MatterDocumentActivityUserFrom>.NewConfig();
    }
}