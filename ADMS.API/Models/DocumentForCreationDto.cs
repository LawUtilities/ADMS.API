using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Document DTO used to create new documents
    /// </summary>
    public class DocumentForCreationDto
    {
        /// <summary>
        /// Filename of the document to be created
        /// </summary>
        [Required]
        public required string FileName { get; set; }

        /// <summary>
        /// Extension for the file in question
        /// </summary>
        [Required]
        public required string Extension { get; set; }

        /// <summary>
        /// indicates if the document is checkedout
        /// </summary>
        public bool IsCheckedOut { get; set; }

        /// <summary>
        /// revisions assigned to the document
        /// </summary>
        public ICollection<RevisionDto> Revisions { get; set; } = [];
    }
}
