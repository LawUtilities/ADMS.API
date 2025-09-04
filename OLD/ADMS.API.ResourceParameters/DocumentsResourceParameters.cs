namespace ADMS.API.ResourceParameters
{
    /// <summary>
    /// Resource Parameters for Documents API
    /// </summary>
    public class DocumentsResourceParameters : PagedResourceParameters
    {
        /// <summary>
        /// FileName to search
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Search query to run
        /// </summary>
        public string? SearchQuery { get; set; }
    }
}
