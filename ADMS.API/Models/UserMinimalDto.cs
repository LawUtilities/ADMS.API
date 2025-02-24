using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Minimal User information
    /// </summary>
    public class UserMinimalDto
    {
        /// <summary>
        /// user primary key
        /// </summary>
        [Required]
        public required Guid Id { get; set; }

        /// <summary>
        /// user name
        /// </summary>
        [Required]
        public required string Name { get; set; }
    }
}
