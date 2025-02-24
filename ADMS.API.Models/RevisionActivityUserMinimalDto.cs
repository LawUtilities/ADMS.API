using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Revision Activity User
    /// </summary>
    public class RevisionActivityUserMinimalDto
    {
        /// <summary>
        /// Revision Id
        /// </summary>
        [Required]
        public required Guid RevisionId { get; set; }

        /// <summary>
        /// Revision
        /// </summary>
        [Required]
        public required RevisionMinimalDto Revision { get; set; }

        /// <summary>
        /// Activity Id
        /// </summary>
        [Required]
        public required Guid RevisionActivityId { get; set; }

        /// <summary>
        /// RevisionActivity
        /// </summary>
        [Required]
        public required RevisionActivityMinimalDto RevisionActivity { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        [Required]
        public required Guid UserId { get; set; }

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
