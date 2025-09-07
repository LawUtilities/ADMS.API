using ADMS.API.Entities;
using ADMS.API.Models;

using Mapster;

namespace ADMS.API.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="DocumentActivity"/>, <see cref="DocumentActivityDto"/>, and <see cref="DocumentActivityMinimalDto"/>.
/// </summary>
public class DocumentActivityProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentActivityProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public DocumentActivityProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<DocumentActivity, DocumentActivityDto>.NewConfig();
        TypeAdapterConfig<DocumentActivity, DocumentActivityMinimalDto>.NewConfig();

        // DTO to Entity mapping
        TypeAdapterConfig<DocumentActivityDto, DocumentActivity>.NewConfig();
    }
}