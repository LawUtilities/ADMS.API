using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Document Activity User join
    /// </summary>
    public class DocumentActivityUserMinimalDto
    {
        /// <summary>
        /// Document Id
        /// </summary>
        [Required]
        public required Guid DocumentId { get; set; }

        /// <summary>
        /// DocumentActivity
        /// </summary>
        [Required]
        public required DocumentActivityMinimalDto DocumentActivity { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [Required]
        public required UserMinimalDto User { get; set; }

        /// <summary>
        /// entry created at
        /// </summary>
        [Required]
        public required DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// Local timezone Creation Date
        /// </summary>
        public string LocalCreatedAtDateString
        {
            get
            {
                return CreatedAt.ToLocalTime().ToString("dddd, dd MMMM yyyy HH:mm:ss");
            }
        }
    }
}
