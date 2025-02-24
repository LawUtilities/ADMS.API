using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities
{
    /// <summary>
    /// Document Entity Framework class
    /// </summary>
    public class Document
    {
        /// <summary>
        /// ID, key field
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// file name of the document
        /// </summary>
        [Required]
        [MaxLength(128)]
        [DataType(DataType.Text)]
        public required string FileName { get; set; }

        /// <summary>
        /// extension of the file
        /// </summary>
        [Required]
        [MaxLength(5)]
        [DataType(DataType.Text)]
        public required string Extension { get; set; }

        /// <summary>
        /// Matter Id linked to the document
        /// </summary>
        [Required]
        public required Guid MatterId { get; set; }

        /// <summary>
        /// Matter the document is linked to
        /// </summary>
        [Required]
        [ForeignKey("MatterId")]
        public required Matter Matter { get; set; }

        /// <summary>
        /// indicates if the document is checkedout
        /// </summary>
        public bool IsCheckedOut { get; set; }

        /// <summary>
        /// Identifies if this document has been deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Document revisions
        /// </summary>
        public ICollection<Revision> Revisions { get; set; } = new HashSet<Revision>();

        /// <summary>
        /// Matter document activities
        /// </summary>
        public ICollection<DocumentActivityUser> DocumentActivityUsers { get; set; } = new HashSet<DocumentActivityUser>();

        /// <summary>
        /// MatterDocumentActivityUser
        /// </summary>
        public ICollection<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; } = new HashSet<MatterDocumentActivityUserFrom>();

        /// <summary>
        /// MatterDocumentActivityUser
        /// </summary>
        public ICollection<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; } = new HashSet<MatterDocumentActivityUserTo>();
    }
}
