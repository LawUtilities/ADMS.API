using ADMS.API.Entities;
using ADMS.API.Models;

using Mapster;

namespace ADMS.Application.Profiles;

/// <summary>
///     MapsterMapper profile for mapping between <see cref="User"/>, <see cref="UserDto"/>, and <see cref="UserMinimalDto"/>.
/// </summary>
public class UserProfile
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserProfile"/> class and configures entity-to-DTO mappings.
    /// </summary>
    public UserProfile()
    {
        // Entity to DTO mappings
        TypeAdapterConfig<User, UserDto>.NewConfig()
            .Map(dest => dest.MatterActivityUsers, src => src.MatterActivityUsers)
            .Map(dest => dest.MatterDocumentActivityUsersFrom, src => src.MatterDocumentActivityUsersFrom)
            .Map(dest => dest.MatterDocumentActivityUsersTo, src => src.MatterDocumentActivityUsersTo);

        TypeAdapterConfig<User, UserMinimalDto>.NewConfig();

        // DTO to Entity mappings
        TypeAdapterConfig<UserDto, User>.NewConfig()
            .Map(dest => dest.MatterActivityUsers, src => src.MatterActivityUsers)
            .Map(dest => dest.MatterDocumentActivityUsersFrom, src => src.MatterDocumentActivityUsersFrom)
            .Map(dest => dest.MatterDocumentActivityUsersTo, src => src.MatterDocumentActivityUsersTo);
        TypeAdapterConfig<UserMinimalDto, User>.NewConfig();
    }
}