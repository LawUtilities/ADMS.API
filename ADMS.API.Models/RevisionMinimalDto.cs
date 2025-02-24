using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Revision DTO
    /// </summary>
    public class RevisionMinimalDto
    {
        /// <summary>
        /// Revision primary key
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Revision number for the specified document
        /// </summary>
        [Required]
        public int RevisionId { get; set; }

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

        /// <summary>
        /// Local Creation Date string
        /// </summary>
        public string LocalCreationDateString
        {
            get
            {
                return CreationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");
            }
        }

        /// <summary>
        /// Local Modification Date string
        /// </summary>
        public string LocalModificationDateString
        {
            get
            {
                return ModificationDate.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");
            }
        }
    }
}