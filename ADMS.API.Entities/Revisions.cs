using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Entities
{
    /// <summary>
    /// document revisions
    /// </summary>
    public class Revision
    {
        /// <summary>
        /// Revision primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }

        /// <summary>
        /// Revision number for the specified document
        /// </summary>
        public int RevisionId { get; set; }

        /// <summary>
        /// Creation date for the revision
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public required DateTime CreationDate { get; set; }

        /// <summary>
        /// Revision modification date
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public required DateTime ModificationDate { get; set; }

        /// <summary>
        /// Document ID linked to this revision
        /// </summary>
        public required Guid DocumentId { get; set; }

        /// <summary>
        /// Document linked to this revision
        /// </summary>
        [ForeignKey("DocumentId")]
        public required Document Document { get; set; }

        /// <summary>
        /// Indicates if a revision is deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Revision Activity User
        /// </summary>
        public ICollection<RevisionActivityUser> RevisionActivityUsers { get; set; } = new HashSet<RevisionActivityUser>();
    }
}
