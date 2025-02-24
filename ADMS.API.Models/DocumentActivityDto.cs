using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Document Activity List
    /// </summary>
    /// <remarks>
    /// Document Activity Constructor
    /// </remarks>
    public class DocumentActivityDto
    {
        /// <summary>
        /// Document Activity primary key
        /// </summary>
        [Required]
        public required Guid Id { get; set; }

        /// <summary>
        /// Document Activity
        /// </summary>
        [Required]
        public required string Activity { get; set; }

        /// <summary>
        /// Matter document activities
        /// </summary>
        public ICollection<DocumentActivityUserDto> DocumentActivityUsers { get; set; } = [];
    }
}
