using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Document For Update Dto
    /// </summary>
    public class DocumentForUpdateDto
    {
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
    }
}
