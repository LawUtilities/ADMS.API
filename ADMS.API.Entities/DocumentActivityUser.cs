using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities
{
    /// <summary>
    /// Document Activity User join
    /// </summary>
    public class DocumentActivityUser
    {
        /// <summary>
        /// Document Id
        /// </summary>
        [Key]
        public required Guid DocumentId { get; set; }

        /// <summary>
        /// Document
        /// </summary>
        [Required]
        [ForeignKey("DocumentId")]
        public required Document Document { get; set; }

        /// <summary>
        /// Activity Id
        /// </summary>
        [Key]
        public required Guid DocumentActivityId { get; set; }

        /// <summary>
        /// DocumentActivity
        /// </summary>
        [Required]
        [ForeignKey("DocumentActivityId")]
        public required DocumentActivity DocumentActivity { get; set; }

        /// <summary>
        /// User Id
        /// </summary>
        [Key]
        public required Guid UserId { get; set; }

        /// <summary>
        /// User
        /// </summary>
        [Required]
        [ForeignKey("UserId")]
        public required User User { get; set; }

        /// <summary>
        /// entry created at
        /// </summary>
        [Key]
        public required DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    }
}
