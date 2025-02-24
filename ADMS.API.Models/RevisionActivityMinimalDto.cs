using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Revision Activity
    /// </summary>
    /// <remarks>
    /// Revision Activity constructor
    /// </remarks>
    public class RevisionActivityMinimalDto
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
    }
}
