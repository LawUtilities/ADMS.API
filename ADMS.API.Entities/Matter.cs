using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities
{
    /// <summary>
    /// Matter for document storage
    /// </summary>
    public class Matter
    {
        /// <summary>
        /// Matter primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }

        /// <summary>
        /// Matter description
        /// </summary>
        [Required]
        [StringLength(128)]
        [DataType(DataType.Text)]
        public required string Description { get; set; }

        /// <summary>
        /// Identifies if the matter is archived
        /// </summary>
        public bool IsArchived { get; set; }

        /// <summary>
        /// Identifies if the Matter has been deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Creation date of the matter
        /// </summary>
        [Required]
        [DataType(DataType.DateTime)]
        public required DateTime CreationDate { get; set; }

        /// <summary>
        /// Documents linked to the matter
        /// </summary>
        public ICollection<Document> Documents { get; set; } = new HashSet<Document>();

        /// <summary>
        /// Matter activity user
        /// </summary>
        public ICollection<MatterActivityUser> MatterActivityUsers { get; set; } = new HashSet<MatterActivityUser>();

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
