using ADMS.Domain.Entities;
using ADMS.API.Models;

using Mapster;

namespace ADMS.Application.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="MatterDocumentActivity"/>, <see cref="MatterDocumentActivityDto"/>, and <see cref="MatterDocumentActivityMinimalDto"/>.
/// </summary>
public class MatterDocumentActivityProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="MatterDocumentActivityProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public MatterDocumentActivityProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<MatterDocumentActivity, MatterDocumentActivityDto>.NewConfig();
        TypeAdapterConfig<MatterDocumentActivity, MatterDocumentActivityMinimalDto>.NewConfig();

        // DTO to Entity mapping
        TypeAdapterConfig<MatterDocumentActivityDto, MatterDocumentActivity>.NewConfig();
    }
}