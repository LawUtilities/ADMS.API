using Microsoft.EntityFrameworkCore;

namespace ADMS.API.Helpers
{
    /// <summary>
    /// Represents a paged list of items with pagination metadata.
    /// </summary>
    /// <typeparam name="T">The type of items in the list.</typeparam>
    public class PagedList<T> : List<T>
    {
        /// <summary>
        /// Gets the current page number (1-based).
        /// </summary>
        public int CurrentPage { get; }

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Gets the size of each page.
        /// </summary>
        public int PageSize { get; }

        /// <summary>
        /// Gets the total number of items.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Gets a value indicating whether there is a previous page.
        /// </summary>
        public bool HasPrevious => CurrentPage > 1;

        /// <summary>
        /// Gets a value indicating whether there is a next page.
        /// </summary>
        public bool HasNext => CurrentPage < TotalPages;

        /// <summary>
        /// Initializes a new instance of the <see cref="PagedList{T}"/> class.
        /// </summary>
        public PagedList(List<T> items, int count, int pageNumber, int pageSize)
            : base(items)
        {
            (TotalCount, PageSize, CurrentPage, TotalPages) = (count, pageSize, pageNumber, pageSize > 0 ? (int)Math.Ceiling(count / (double)pageSize) : 0);
        }

        /// <summary>
        /// Asynchronously creates a paged list from an <see cref="IQueryable{T}"/> source.
        /// </summary>
        /// <param name="source">The source queryable.</param>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The size of each page.</param>
        /// <returns>A <see cref="PagedList{T}"/> containing the items and pagination metadata.</returns>
        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            ArgumentNullException.ThrowIfNull(source);
            if (pageNumber < 1)
                throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");
            if (pageSize < 1)
                throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");

            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        /// <summary>
        /// Creates an empty paged list with the specified page number and page size.
        /// </summary>
        /// <param name="pageNumber">The current page number (1-based).</param>
        /// <param name="pageSize">The size of each page.</param>
        /// <returns>An empty <see cref="PagedList{T}"/>.</returns>
        public static PagedList<T> CreateEmpty(int pageNumber, int pageSize)
        {
            if (pageNumber < 1) throw new ArgumentOutOfRangeException(nameof(pageNumber), "Page number must be greater than 0.");
            if (pageSize < 1) throw new ArgumentOutOfRangeException(nameof(pageSize), "Page size must be greater than 0.");

            return new PagedList<T>([], 0, pageNumber, pageSize);
        }

    }
}
