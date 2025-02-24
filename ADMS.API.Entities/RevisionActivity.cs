using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ADMS.API.Entities
{
    /// <summary>
    /// Revision Activity
    /// </summary>
    /// <remarks>
    /// Revision Activity constructor
    /// </remarks>
    public class RevisionActivity()
    {
        /// <summary>
        /// Revision Activity primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// Activity description
        /// </summary>
        [Required]
        [StringLength(50)]
        [DataType(DataType.Text)]
        public required string Activity { get; set; }

        /// <summary>
        /// Revision Activity User
        /// </summary>
        public ICollection<RevisionActivityUser> RevisionActivityUsers { get; set; } = new HashSet<RevisionActivityUser>();
    }
}
