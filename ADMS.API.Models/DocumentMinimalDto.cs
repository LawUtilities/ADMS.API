using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Document DTO
    /// </summary>
    public class DocumentMinimalDto
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
    }
}
