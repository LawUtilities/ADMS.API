using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities
{
    /// <summary>
    /// shared type entity for many to many tables
    /// </summary>
    public class MatterActivityUser
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
        public Matter? Matter { get; set; }

        /// <summary>
        /// Matter Activity Id
        /// </summary>
        [Key]
        public required Guid MatterActivityId { get; set; }

        /// <summary>
        /// MatterActivity
        /// </summary>
        [ForeignKey("MatterActivityId")]
        public MatterActivity? MatterActivity { get; set; }

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
        /// [Key]
        public required DateTime CreatedAt { get; set; } = DateTime.Now.ToUniversalTime();
    }
}
