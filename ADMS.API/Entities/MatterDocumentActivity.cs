using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities
{
    /// <summary>
    /// Matter document activities
    /// </summary>
    /// <remarks>
    /// matter document activity constructor
    /// </remarks>
    public class MatterDocumentActivity()
    {
        /// <summary>
        /// Matter Document Activity Primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }

        /// <summary>
        /// Activity description
        /// </summary>
        [Required]
        [StringLength(50)]
        public required string Activity { get; set; }

        /// <summary>
        /// Matter MatterDocumentActivity User
        /// </summary>
        public ICollection<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; } = new HashSet<MatterDocumentActivityUserFrom>();

        /// <summary>
        /// Matter MatterDocumentActivity User
        /// </summary>
        public ICollection<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; } = new HashSet<MatterDocumentActivityUserTo>();
    }
}
