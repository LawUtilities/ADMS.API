namespace ADMS.API.Models
{
    /// <summary>
    /// Document Audit Dto
    /// </summary>
    public class DocumentAuditDto
    {
        /// <summary>
        /// List of document activity by users
        /// </summary>
        public IEnumerable<DocumentActivityUserMinimalDto> DocumentActivityUserAudit { get; set; } = [];

        /// <summary>
        /// List matter document from activity by users
        /// </summary>
        public IEnumerable<MatterDocumentActivityUserMinimalDto> MatterDocumentActivityUserFromAudit { get; set; } = [];

        /// <summary>
        /// List of matter document to activity by users
        /// </summary>
        public IEnumerable<MatterDocumentActivityUserMinimalDto> MatterDocumentActivityUserToAudit { get; set; } = [];
    }
}
