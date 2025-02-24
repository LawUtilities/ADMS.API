using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Models
{
    /// <summary>
    /// MatterDocumentActivityUser table
    /// </summary>
    public class MatterDocumentActivityUserMinimalDto
    {
        /// <summary>
        /// Matter Id
        /// </summary>
        [Key]
        public required Guid MatterId { get; set; }

        /// <summary>
        /// Matter
        /// </summary>
        [ForeignKey("MatterId")]
        public MatterMinimalDto? Matter { get; set; }

        /// <summary>
        /// What documentId was this action taken from
        /// </summary>
        [Required]
        public required Guid DocumentId { get; set; }

        /// <summary>
        /// Document Activity Id
        /// </summary>
        [Key]
        public required Guid MatterDocumentActivityId { get; set; }

        /// <summary>
        /// MatterDocumentActivity
        /// </summary>
        [ForeignKey("MatterDocumentActivityId")]
        public MatterDocumentActivityMinimalDto? MatterDocumentActivity { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        [Key]
        public required Guid UserId { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey("UserId")]
        public UserMinimalDto? User { get; set; }

        /// <summary>
        /// entry created at
        /// </summary>
        [Key]
        public required DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();

        /// <summary>
        /// local created at timestring
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
