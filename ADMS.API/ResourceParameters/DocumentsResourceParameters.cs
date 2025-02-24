namespace ADMS.API.ResourceParameters
{
    /// <summary>
    /// Resource Parameters for Documents API
    /// </summary>
    public class DocumentsResourceParameters
    {
        /// <summary>
        /// Maximum page size
        /// </summary>
        const int maxPageSize = 50;

        /// <summary>
        /// FileName to search
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Search query to run
        /// </summary>
        public string? SearchQuery { get; set; }

        /// <summary>
        /// Page Number to retrieve
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Page Size
        /// </summary>
        private int _pageSize = 10;

        /// <summary>
        /// Page Size member
        /// </summary>
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value;
        }

        /// <summary>
        /// What to order by
        /// </summary>
        public string OrderBy { get; set; } = "FileName";

        /// <summary>
        /// Fields to use
        /// </summary>
        public string? Fields { get; set; }
    }
}
