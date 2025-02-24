using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities
{
    /// <summary>
    /// Matter DocumentActivity User linkage table
    /// </summary>
    public class MatterDocumentActivityUserTo
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
        public Matter? Matter { get; set; }

        /// <summary>
        /// What documentId was this action taken from
        /// </summary>
        [Key]
        public required Guid DocumentId { get; set; }

        /// <summary>
        /// Matter document copied / moved from
        /// </summary>
        [ForeignKey("DocumentId")]
        public Document? Document { get; set; }

        /// <summary>
        /// Document Activity Id
        /// </summary>
        [Key]
        public required Guid MatterDocumentActivityId { get; set; }

        /// <summary>
        /// MatterDocumentActivity
        /// </summary>
        [ForeignKey("MatterDocumentActivityId")]
        public MatterDocumentActivity? MatterDocumentActivity { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        [Key]
        public required Guid UserId { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [ForeignKey("UserId")]
        public User? User { get; set; }

        /// <summary>
        /// entry created at
        /// </summary>
        [Key]
        public required DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    }
}
