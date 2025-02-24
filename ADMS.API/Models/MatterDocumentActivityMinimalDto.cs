using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Matter document activities
    /// </summary>
    /// <remarks>
    /// matter document activity constructor
    /// </remarks>
    public class MatterDocumentActivityMinimalDto
    {
        /// <summary>
        /// Matter Document Activity Primary key
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
