using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Matter Dto
    /// </summary>
    public class MatterMinimalDto
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
