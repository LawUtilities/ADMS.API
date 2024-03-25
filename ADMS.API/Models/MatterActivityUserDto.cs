using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Models
{
    /// <summary>
    /// shared type entity for many to many tables
    /// </summary>
    public class MatterActivityUserDto
    {
        /// <summary>
        /// Matter Id
        /// </summary>
        [Key]
        public required Guid MatterId { get; set; }

        /// <summary>
        /// Document
        /// </summary>
        [ForeignKey("MatterId")]
        public MatterWithoutDocumentsDto? Matter { get; set; }

        /// <summary>
        /// Matter Activity Id
        /// </summary>
        [Key]
        public required Guid MatterActivityId { get; set; }

        /// <summary>
        /// MatterActivity
        /// </summary>
        [ForeignKey("MatterActivityId")]
        public MatterActivityDto? MatterActivity { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        [Key]
        public required Guid UserId { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey("UserId")]
        public UserDto? User { get; set; }

        /// <summary>
        /// entry created at
        /// </summary>
        /// [Key]
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
