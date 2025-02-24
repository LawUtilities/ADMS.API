namespace ADMS.API.Services
{
    /// <summary>
    /// Pagination Metadata class
    /// </summary>
    public class PaginationMetadata
    {
        /// <summary>
        /// Toital Item Count
        /// </summary>
        public int TotalItemCount { get; set; }

        /// <summary>
        /// Total Page Count
        /// </summary>
        public int TotalPageCount { get; set; }

        /// <summary>
        /// Page Size
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Current Page Number
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Pagination Metadata Constructor
        /// </summary>
        /// <param name="totalItemCount">total item count</param>
        /// <param name="pageSize">page size</param>
        /// <param name="currentPage">current page</param>
        public PaginationMetadata(int totalItemCount, int pageSize, int currentPage)
        {
            TotalItemCount = totalItemCount;
            PageSize = pageSize;
            CurrentPage = currentPage;
            TotalPageCount = (int)Math.Ceiling(totalItemCount / (double)pageSize);
        }
    }
}
