using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADMS.API.Entities
{
    /// <summary>
    /// Matter activity
    /// </summary>
    /// <remarks>
    /// MatterActivity Constructor
    /// </remarks>
    public class MatterActivity()
    {
        /// <summary>
        /// Matter Activity Primary Key
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }

        /// <summary>
        /// name of the activity being undertaken
        /// </summary>
        [Required]
        [DataType(DataType.Text)]
        public required string Activity { get; set; }

        /// <summary>
        /// Matter activity user
        /// </summary>
        public ICollection<MatterActivityUser> MatterActivityUsers { get; set; } = new HashSet<MatterActivityUser>();
    }
}
