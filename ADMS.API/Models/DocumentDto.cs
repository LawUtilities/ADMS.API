using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Document DTO
    /// </summary>
    public class DocumentDto
    {
        /// <summary>
        /// Id of the document
        /// </summary>
        [Required]
        public required Guid Id { get; set; }

        /// <summary>
        /// Filename of the document
        /// </summary>
        [Required]
        public required string FileName { get; set; }

        /// <summary>
        /// extension of the file
        /// </summary>
        [Required]
        public required string Extension { get; set; }

        /// <summary>
        /// indicates if the document is checkedout
        /// </summary>
        public bool IsCheckedOut { get; set; }

        /// <summary>
        /// is the file deleted
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// revisions assigned to the document
        /// </summary>
        public ICollection<RevisionDto> Revisions { get; set; } = new HashSet<RevisionDto>();

        /// <summary>
        /// document activities user
        /// </summary>
        public ICollection<DocumentActivityUserDto> DocumentActivityUsers { get; set; } = new HashSet<DocumentActivityUserDto>();

        /// <summary>
        /// MatterDocumentActivityUserFrom
        /// </summary>
        public ICollection<MatterDocumentActivityUserFromDto> MatterDocumentActivityUsersFrom { get; set; } = new HashSet<MatterDocumentActivityUserFromDto>();

        /// <summary>
        /// MatterDocumentActivityUserTo
        /// </summary>
        public ICollection<MatterDocumentActivityUserToDto> MatterDocumentActivityUsersTo { get; set; } = new HashSet<MatterDocumentActivityUserToDto>();
    }
}
