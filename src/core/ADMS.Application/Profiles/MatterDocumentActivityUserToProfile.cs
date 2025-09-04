using ADMS.Domain.Entities;
using ADMS.Application.DTOs;

using Mapster;

namespace ADMS.Application.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="MatterDocumentActivityUserTo"/>, <see cref="MatterDocumentActivityUserToDto"/>, and <see cref="MatterDocumentActivityUserMinimalDto"/>.
/// </summary>
public class MatterDocumentActivityUserToProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MatterDocumentActivityUserToProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public MatterDocumentActivityUserToProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<MatterDocumentActivityUserTo, MatterDocumentActivityUserToDto>.NewConfig();
        TypeAdapterConfig<MatterDocumentActivityUserTo, MatterDocumentActivityUserMinimalDto>.NewConfig();

        // DTO to minimal DTO mapping
        TypeAdapterConfig<MatterDocumentActivityUserToDto, MatterDocumentActivityUserMinimalDto>.NewConfig();
    }
}