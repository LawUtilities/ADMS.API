using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// User information
    /// </summary>
    public class UserDto
    {
        /// <summary>
        /// user primary key
        /// </summary>
        [Required]
        public required Guid Id { get; set; }

        /// <summary>
        /// user name
        /// </summary>
        [Required]
        public required string Name { get; set; }

        /// <summary>
        /// Matter activity user
        /// </summary>
        public ICollection<MatterActivityUserDto> MatterActivityUsers { get; set; } = [];

        /// <summary>
        /// Document Activity User
        /// </summary>
        public ICollection<DocumentActivityUserDto> DocumentActivityUsers { get; set; } = [];

        /// <summary>
        /// Revision Activity User
        /// </summary>
        public ICollection<RevisionActivityUserDto> RevisionActivityUsers { get; set; } = [];

        /// <summary>
        /// MatterDocumentActivityUserFrom
        /// </summary>
        public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = [];

        /// <summary>
        /// MatterDocumentActivityUserTo
        /// </summary>
        public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; set; } = [];
    }
}
