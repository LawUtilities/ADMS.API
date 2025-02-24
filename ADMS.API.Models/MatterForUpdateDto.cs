using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Matter For Update Dto
    /// </summary>
    public class MatterForUpdateDto
    {
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
        /// Creation date of the matter
        /// </summary>
        [Required]
        public required DateTime CreationDate { get; set; } = DateTime.Now.ToUniversalTime();
    }
}
