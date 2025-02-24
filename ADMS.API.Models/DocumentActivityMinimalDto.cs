using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Document Activity List
    /// </summary>
    /// <remarks>
    /// Document Activity Constructor
    /// </remarks>
    public class DocumentActivityMinimalDto
    {
        /// <summary>
        /// Document Activity
        /// </summary>
        [Required]
        public required string Activity { get; set; }
    }
}
