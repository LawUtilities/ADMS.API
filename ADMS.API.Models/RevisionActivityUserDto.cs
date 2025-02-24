using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Models
{
    /// <summary>
    /// Revision Activity User
    /// </summary>
    public class RevisionActivityUserDto
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
        public required RevisionDto Revision { get; set; }

        /// <summary>
        /// Activity Id
        /// </summary>
        [Required]
        public required Guid RevisionActivityId { get; set; }

        /// <summary>
        /// RevisionActivity
        /// </summary>
        [Required]
        public required RevisionActivityDto RevisionActivity { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        [Required]
        public required Guid UserId { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [Required]
        public required UserDto User { get; set; }

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
