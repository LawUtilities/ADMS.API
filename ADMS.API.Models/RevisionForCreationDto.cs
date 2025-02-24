using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Revision Dto for creation of new revision
    /// </summary>
    public class RevisionForCreationDto
    {
        /// <summary>
        /// Revision number for the specified document
        /// </summary>
        [Required]
        public required int RevisionId { get; set; }

        /// <summary>
        /// Creation date for the revision
        /// </summary>
        [Required]
        public DateTime CreationDate { get; set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// Revision modification date
        /// </summary>
        [Required]
        public DateTime ModificationDate { get; set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// indicates if the revision is deleted
        /// </summary>
        public bool IsDeleted { get; set; } = false;

    }
}
