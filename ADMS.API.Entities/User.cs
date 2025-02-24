using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities
{
    /// <summary>
    /// User information
    /// </summary>
    public class User
    {
        /// <summary>
        /// user primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// user name
        /// </summary>
        [Required]
        [StringLength(50)]
        [DataType(DataType.Text)]
        public required string Name { get; set; }

        /// <summary>
        /// Matter activity user
        /// </summary>
        public ICollection<MatterActivityUser> MatterActivityUsers { get; set; } = new HashSet<MatterActivityUser>();

        /// <summary>
        /// Document Activity User
        /// </summary>
        public ICollection<DocumentActivityUser> DocumentActivityUsers { get; set; } = new HashSet<DocumentActivityUser>();

        /// <summary>
        /// Revision Activity User
        /// </summary>
        public ICollection<RevisionActivityUser> RevisionActivityUsers { get; set; } = new HashSet<RevisionActivityUser>();

        /// <summary>
        /// Matter Document Activity User
        /// </summary>
        public ICollection<MatterDocumentActivityUserFrom> MatterDocumentActivityUsersFrom { get; set; } = new HashSet<MatterDocumentActivityUserFrom>();

        /// <summary>
        /// Matter Document Activity User
        /// </summary>
        public ICollection<MatterDocumentActivityUserTo> MatterDocumentActivityUsersTo { get; set; } = new HashSet<MatterDocumentActivityUserTo>();
    }
}
