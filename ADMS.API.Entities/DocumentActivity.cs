using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities
{
    /// <summary>
    /// Document Activity List
    /// </summary>
    /// <remarks>
    /// Document Activity Constructor
    /// </remarks>
    public class DocumentActivity()
    {
        /// <summary>
        /// Document Activity primary key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }

        /// <summary>
        /// Document Activity
        /// </summary>
        [Required]
        [StringLength(50)]
        [DataType(DataType.Text)]
        public required string Activity { get; set; }

        /// <summary>
        /// Matter document activities
        /// </summary>
        public ICollection<DocumentActivityUser> DocumentActivityUsers { get; set; } = new HashSet<DocumentActivityUser>();
    }
}
