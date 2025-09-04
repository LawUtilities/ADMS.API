namespace ADMS.API.Services;

/// <summary>
///     Pagination Metadata class
/// </summary>
public class PaginationMetadata(int totalItemCount, int pageSize, int currentPage)
{
    /// <summary>
    ///     Total Item Count
    /// </summary>
    public int TotalItemCount { get; } = totalItemCount;

    /// <summary>
    ///     Total Page Count
    /// </summary>
    public int TotalPageCount { get; } = (int)Math.Ceiling(totalItemCount / (double)pageSize);

    /// <summary>
    ///     Page Size
    /// </summary>
    public int PageSize { get; } = pageSize;

    /// <summary>
    ///     Current Page Number
    /// </summary>
    public int CurrentPage { get; } = currentPage;
}