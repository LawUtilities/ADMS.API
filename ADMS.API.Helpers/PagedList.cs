using Microsoft.EntityFrameworkCore;

namespace ADMS.API.Helpers;

/// <summary>
/// Paged List
/// </summary>
/// <typeparam name="T">returns list of type T</typeparam>
public class PagedList<T> : List<T>
{
    /// <summary>
    /// Current page being retrieved
    /// </summary>
    public int CurrentPage { get; private set; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; private set; }

    /// <summary>
    /// Page Size to retrieve
    /// </summary>
    public int PageSize { get; private set; }

    /// <summary>
    /// Total record count
    /// </summary>
    public int TotalCount { get; private set; }

    /// <summary>
    /// determines if a previous page exists
    /// </summary>
    public bool HasPrevious => CurrentPage > 1;

    /// <summary>
    /// determines if next page exists
    /// </summary>
    public bool HasNext => CurrentPage < TotalPages;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="items">list items</param>
    /// <param name="count">total records</param>
    /// <param name="pageNumber">page number to retrieve</param>
    /// <param name="pageSize">Page size to return</param>
    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        CurrentPage = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        AddRange(items);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="pageNumber"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    public static async Task<PagedList<T>> CreateAsync(
        IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = source.Count();
        var items = await source.Skip((pageNumber - 1) * pageSize)
            .Take(pageSize).ToListAsync();
        return new PagedList<T>(items, count, pageNumber, pageSize);
    }
}

