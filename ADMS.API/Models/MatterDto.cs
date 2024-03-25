using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Matter Dto
    /// </summary>
    public class MatterDto
    {
        /// <summary>
        /// Matter primary key
        /// </summary>
        [Required]
        public required Guid Id { get; set; }

        /// <summary>
        /// Matter description
        /// </summary>
        [Required]
        public required string Description { get; set; }

        /// <summary>
        /// Identifies if the matter is archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Identifies if the Matter has been deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Creation date of the matter
        /// </summary>
        [Required]
        public required DateTime CreationDate { get; set; }

        /// <summary>
        /// Documents linked to the matter
        /// </summary>
        public ICollection<DocumentWithoutRevisionsDto> Documents { get; set; } = new HashSet<DocumentWithoutRevisionsDto>();

        /// <summary>
        /// Matter activity user
        /// </summary>
        public ICollection<MatterActivityUserDto> MatterActivityUsers { get; set; } = new HashSet<MatterActivityUserDto>();

        /// <summary>
        /// MatterDocumentActivityUserFrom
        /// </summary>
        public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = new HashSet<MatterDocumentActivityUserFromDto>();

        /// <summary>
        /// MatterDocumentActivityUserTo
        /// </summary>
        public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; set; } = new HashSet<MatterDocumentActivityUserToDto>();

        /// <summary>
        /// Local timezone Creation Date
        /// </summary>
        public string LocalCreationDateString
        {
            get
            {
                return CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");
            }
        }
    }
}
