using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Document DTO without Revisions included
    /// </summary>
    public class DocumentWithoutRevisionsDto
    {
        /// <summary>
        /// ID for the document
        /// </summary>
        [Required]
        public required Guid Id { get; set; }

        /// <summary>
        /// filename for the document
        /// </summary>
        [Required]
        public required string FileName { get; set; }

        /// <summary>
        /// file extension for the document
        /// </summary>
        [Required]
        public required string Extension { get; set; }

        /// <summary>
        /// indicates if the document is checkedout
        /// </summary>
        public bool IsCheckedOut { get; set; }

        /// <summary>
        /// indicates if the document is deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// document activities user
        /// </summary>
        public ICollection<DocumentActivityUserDto> DocumentActivityUsers { get; set; } = [];

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
