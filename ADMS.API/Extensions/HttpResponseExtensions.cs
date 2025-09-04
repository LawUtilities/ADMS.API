using ADMS.API.Helpers;

using System.Text.Json;

namespace ADMS.API.Extensions;

/// <summary>
/// Extension methods for HttpResponse to enhance functionality for API responses.
/// </summary>
/// <remarks>
/// This static class provides extension methods for the HttpResponse class to add
/// common functionality such as pagination metadata headers. These extensions
/// help maintain consistency across API endpoints and reduce code duplication.
/// </remarks>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Adds pagination metadata to the HTTP response headers as JSON.
    /// </summary>
    /// <remarks>
    /// This method serializes pagination information from a PagedList and adds it
    /// to the response headers under the 'X-Pagination' key. This allows clients
    /// to access pagination metadata without needing to inspect the response body.
    /// 
    /// The pagination metadata includes:
    /// - TotalCount: Total number of items across all pages
    /// - PageSize: Number of items per page
    /// - CurrentPage: Current page number (1-based)
    /// - TotalPages: Total number of pages
    /// - HasNext: Boolean indicating if there is a next page
    /// - HasPrevious: Boolean indicating if there is a previous page
    /// 
    /// The metadata is serialized using System.Text.Json with camelCase naming policy
    /// to maintain consistency with the API's JSON response format.
    /// </remarks>
    /// <typeparam name="T">The type of items in the paged list.</typeparam>
    /// <param name="response">The HTTP response to add metadata to. Cannot be null.</param>
    /// <param name="pagedList">The paged list containing pagination metadata. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="response"/> or <paramref name="pagedList"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// var pagedResult = await _repository.GetPaginatedItemsAsync(pageNumber, pageSize);
    /// Response.AddPaginationMetadata(pagedResult);
    /// return Ok(pagedResult);
    /// </code>
    /// </example>
    public static void AddPaginationMetadata<T>(this HttpResponse response, PagedList<T> pagedList)
    {
        ArgumentNullException.ThrowIfNull(response, nameof(response));
        ArgumentNullException.ThrowIfNull(pagedList, nameof(pagedList));

        // Create pagination metadata object
        var paginationMetadata = new
        {
            pagedList.TotalCount,
            pagedList.PageSize,
            pagedList.CurrentPage,
            pagedList.TotalPages,
            pagedList.HasNext,
            pagedList.HasPrevious
        };

        // Configure JSON serialization options for consistency
        JsonSerializerOptions jsonOptions;
        jsonOptions = new JsonSerializerOptions();
        jsonOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        jsonOptions.WriteIndented = false; // Compact JSON for headers

        // Serialize and add to response headers
        var jsonMetadata = JsonSerializer.Serialize(paginationMetadata, jsonOptions);
        response.Headers["X-Pagination"] = jsonMetadata;
    }

    /// <summary>
    /// Adds a correlation ID to the HTTP response headers for request tracing.
    /// </summary>
    /// <remarks>
    /// This method adds a correlation ID to the response headers, which can be used
    /// for request tracing and logging correlation across distributed systems.
    /// If no correlation ID is provided, a new GUID will be generated.
    /// </remarks>
    /// <param name="response">The HTTP response to add the correlation ID to. Cannot be null.</param>
    /// <param name="correlationId">
    /// Optional correlation ID. If null or empty, a new GUID will be generated.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="response"/> is null.
    /// </exception>
    /// <example>
    /// <code>
    /// Response.AddCorrelationId();
    /// // or with a specific correlation ID
    /// Response.AddCorrelationId("12345678-1234-1234-1234-123456789012");
    /// </code>
    /// </example>
    public static void AddCorrelationId(this HttpResponse response, string? correlationId = null)
    {
        ArgumentNullException.ThrowIfNull(response, nameof(response));

        var id = string.IsNullOrWhiteSpace(correlationId)
            ? Guid.NewGuid().ToString()
            : correlationId;

        response.Headers["X-Correlation-ID"] = id;
    }

    /// <summary>
    /// Adds API version information to the HTTP response headers.
    /// </summary>
    /// <remarks>
    /// This method adds version information to the response headers, making it
    /// easier for clients to identify which API version processed their request.
    /// This is particularly useful in versioned APIs for debugging and monitoring.
    /// </remarks>
    /// <param name="response">The HTTP response to add version information to. Cannot be null.</param>
    /// <param name="version">The API version string. Cannot be null or empty.</param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="response"/> or <paramref name="version"/> is null.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="version"/> is empty or whitespace.
    /// </exception>
    /// <example>
    /// <code>
    /// Response.AddApiVersion("1.0");
    /// </code>
    /// </example>
    public static void AddApiVersion(this HttpResponse response, string version)
    {
        ArgumentNullException.ThrowIfNull(response, nameof(response));
        ArgumentException.ThrowIfNullOrWhiteSpace(version, nameof(version));

        response.Headers["X-API-Version"] = version;
    }

    /// <summary>
    /// Adds cache control headers to the HTTP response.
    /// </summary>
    /// <remarks>
    /// This method adds appropriate cache control headers to the response,
    /// allowing fine-grained control over client-side and proxy caching behavior.
    /// This is useful for optimizing performance and reducing server load for
    /// cacheable resources.
    /// </remarks>
    /// <param name="response">The HTTP response to add cache headers to. Cannot be null.</param>
    /// <param name="maxAge">Maximum age for caching in seconds. Must be non-negative.</param>
    /// <param name="isPublic">
    /// If true, allows public caches (proxies) to cache the response.
    /// If false, only private caches (browsers) can cache.
    /// </param>
    /// <param name="mustRevalidate">
    /// If true, forces caches to revalidate with the origin server before serving stale content.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="response"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="maxAge"/> is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Cache for 5 minutes, public cache, must revalidate
    /// Response.AddCacheHeaders(300, isPublic: true, mustRevalidate: true);
    /// 
    /// // No caching
    /// Response.AddCacheHeaders(0, isPublic: false, mustRevalidate: false);
    /// </code>
    /// </example>
    public static void AddCacheHeaders(this HttpResponse response, int maxAge, bool isPublic = true, bool mustRevalidate = false)
    {
        ArgumentNullException.ThrowIfNull(response, nameof(response));
        ArgumentOutOfRangeException.ThrowIfNegative(maxAge, nameof(maxAge));

        var cacheDirectives = new List<string>();

        if (maxAge == 0)
        {
            cacheDirectives.Add("no-cache");
            cacheDirectives.Add("no-store");
        }
        else
        {
            cacheDirectives.Add(isPublic ? "public" : "private");
            cacheDirectives.Add($"max-age={maxAge}");

            if (mustRevalidate)
            {
                cacheDirectives.Add("must-revalidate");
            }
        }

        response.Headers["Cache-Control"] = string.Join(", ", cacheDirectives);
    }
}