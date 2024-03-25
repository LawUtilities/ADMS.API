using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Matter Dto
    /// </summary>
    public class MatterWithoutDocumentsDto
    {
        /// <summary>
        /// Matter Id
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Matter Description
        /// </summary>
        [Required]
        public required string Description { get; set; }

        /// <summary>
        /// Identifies if the matter is archived
        /// </summary>
        public bool IsArchived { get; set; } = false;

        /// <summary>
        /// indicates if the matter is deleted
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Creation date of the matter
        /// </summary>
        [Required]
        public required DateTime CreationDate { get; set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// Matter document activities
        /// </summary>
        public ICollection<MatterActivityUserDto> MatterActivityUsers { get; set; } = new HashSet<MatterActivityUserDto>();

        /// <summary>
        /// MatterDocumentActivityUserFrom
        /// </summary>
        public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = new HashSet<MatterDocumentActivityUserFromDto>();

        /// <summary>
        /// MatterDocumentActivityUserFrom
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