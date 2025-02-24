using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Matter activity
    /// </summary>
    /// <remarks>
    /// MatterActivity Constructor
    /// </remarks>
    public class MatterActivityDto
    {
        /// <summary>
        /// Matter Activity Primary Key
        /// </summary>
        [Required]
        public required Guid Id { get; set; }

        /// <summary>
        /// name of the activity being undertaken
        /// </summary>
        [Required]
        public required string Activity { get; set; }

        /// <summary>
        /// Matter activity user
        /// </summary>
        public ICollection<MatterActivityUserDto> MatterActivityUsers { get; set; } = [];
    }
}
