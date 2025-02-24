using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Revision Activity
    /// </summary>
    /// <remarks>
    /// Revision Activity constructor
    /// </remarks>
    public class RevisionActivityDto
    {
        /// <summary>
        /// Revision Activity primary key
        /// </summary>
        [Required]
        public required Guid Id { get; set; }

        /// <summary>
        /// Activity description
        /// </summary>
        [Required]
        public required string Activity { get; set; }

        /// <summary>
        /// Revision Activity User
        /// </summary>
        public ICollection<RevisionActivityUserDto> RevisionActivityUsers { get; set; } = [];
    }
}
