using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities
{
    /// <summary>
    /// Revision Activity User
    /// </summary>
    public class RevisionActivityUser
    {
        /// <summary>
        /// Revision Id
        /// </summary>
        [Key]
        public required Guid RevisionId { get; set; }

        /// <summary>
        /// Revision
        /// </summary>
        [Required]
        [ForeignKey("RevisionId")]
        public required Revision Revision { get; set; }

        /// <summary>
        /// Activity Id
        /// </summary>
        [Key]
        public required Guid RevisionActivityId { get; set; }

        /// <summary>
        /// RevisionActivity
        /// </summary>
        [Required]
        [ForeignKey("RevisionActivityId")]
        public required RevisionActivity RevisionActivity { get; set; }

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
        [Required]
        public required DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    }
}
