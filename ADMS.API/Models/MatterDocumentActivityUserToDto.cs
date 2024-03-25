using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Models
{
    /// <summary>
    /// MatterDocumentActivityUserTo linkage table
    /// </summary>
    public class MatterDocumentActivityUserToDto
    {
        /// <summary>
        /// Matter Id
        /// </summary>
        [Key]
        public required Guid MatterId { get; set; }

        /// <summary>
        /// Matter document copied / moved from
        /// </summary>
        [ForeignKey("MatterId")]
        public MatterWithoutDocumentsDto? Matter { get; set; }

        /// <summary>
        /// What documentId was this action taken from
        /// </summary>
        [Required]
        public required Guid DocumentId { get; set; }

        /// <summary>
        /// Matter document copied / moved from
        /// </summary>
        [ForeignKey("DocumentId")]
        public DocumentWithoutRevisionsDto? Document { get; set; }

        /// <summary>
        /// Document Activity Id
        /// </summary>
        [Key]
        public required Guid MatterDocumentActivityId { get; set; }

        /// <summary>
        /// MatterDocumentActivity
        /// </summary>
        [ForeignKey("MatterDocumentActivityId")]
        public MatterDocumentActivityDto? MatterDocumentActivity { get; set; }

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
