using ADMS.API.Entities;
using ADMS.API.Models;

using Mapster;

namespace ADMS.API.Profiles;

/// <summary>
///     AutoMapper profile for mapping between <see cref="DocumentActivityUser"/>, <see cref="DocumentActivityUserDto"/>, and <see cref="DocumentActivityUserMinimalDto"/>.
/// </summary>
public class DocumentActivityUserProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DocumentActivityUserProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public DocumentActivityUserProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<DocumentActivityUser, DocumentActivityUserDto>.NewConfig();
        TypeAdapterConfig<DocumentActivityUser, DocumentActivityUserMinimalDto>.NewConfig();

        // DTO to Entity mapping
        TypeAdapterConfig<DocumentActivityUserDto, DocumentActivityUser>.NewConfig();
    }
}