using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Revision Dto for update of new revision
    /// </summary>
    public class RevisionForUpdateDto
    {
        /// <summary>
        /// Revision number for the specified document
        /// </summary>
        [Required]
        public required int RevisionId { get; set; }

        /// <summary>
        /// Creation date for the revision
        /// </summary>
        public DateTime CreationDate { get; set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// Revision modification date
        /// </summary>
        public DateTime ModificationDate { get; set; } = DateTime.Now.ToUniversalTime();

    }
}
