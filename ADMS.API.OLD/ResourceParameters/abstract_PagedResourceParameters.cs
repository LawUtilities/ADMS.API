namespace ADMS.API.ResourceParameters;

/// <summary>
/// Base resource parameters for paginated API endpoints.
/// </summary>
public abstract class PagedResourceParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    /// <summary>
    /// Page number to retrieve.
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Page size to retrieve.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    /// <summary>
    /// Order by field(s).
    /// </summary>
    public string? OrderBy { get; set; }

    /// <summary>
    /// Fields for data shaping.
    /// </summary>
    public string? Fields { get; set; }
}
