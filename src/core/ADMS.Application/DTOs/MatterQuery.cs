using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ADMS.Application.DTOs;

/// <summary>
/// Represents a comprehensive query object for filtering and searching matters in the ADMS legal document management system.
/// </summary>
/// <remarks>
/// The MatterQuery class provides advanced filtering capabilities for matter search operations, specifically designed
/// to support the UI Desktop program's matter search functionality. It enables legal professionals to efficiently
/// locate matters using various criteria while maintaining professional standards and data integrity.
/// 
/// <para><strong>Enhanced Features (.NET 9):</strong></para>
/// <list type="bullet">
/// <item><strong>Advanced Text Search:</strong> Multiple search modes including wildcard, regex, and fuzzy matching</item>
/// <item><strong>Client-Based Filtering:</strong> Search by client information and matter relationships</item>
/// <item><strong>Document-Based Filtering:</strong> Filter matters by document count and document properties</item>
/// <item><strong>User Activity Filtering:</strong> Filter by user activities and audit trail information</item>
/// <item><strong>Pagination Integration:</strong> Full integration with PagedResourceParameters pattern</item>
/// <item><strong>Performance Optimization:</strong> Optimized for large datasets with selective loading</item>
/// </list>
/// 
/// <para><strong>Search Modes:</strong></para>
/// <list type="bullet">
/// <item><strong>Simple:</strong> Basic contains search (default)</item>
/// <item><strong>Exact:</strong> Exact match search</item>
/// <item><strong>StartsWith:</strong> Prefix matching</item>
/// <item><strong>EndsWith:</strong> Suffix matching</item>
/// <item><strong>Wildcard:</strong> Support for * and ? wildcards</item>
/// <item><strong>Regex:</strong> Full regular expression support</item>
/// </list>
/// </remarks>
public class MatterQuery : IValidatableObject, IEquatable<MatterQuery>
{
    #region Enhanced Search Properties

    /// <summary>
    /// Gets or sets the description or search text to filter matters.
    /// </summary>
    /// <remarks>
    /// Enhanced to support multiple search modes and patterns for more flexible matter discovery.
    /// </remarks>
    [StringLength(MatterValidationHelper.MaxDescriptionLength, MinimumLength = 2,
        ErrorMessage = "Search description must be between 2 and 128 characters when provided.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the search mode for description filtering.
    /// </summary>
    /// <remarks>
    /// Specifies how the Description property should be interpreted and applied in the search.
    /// Default is Simple (contains matching).
    /// </remarks>
    public SearchMode SearchMode { get; set; } = SearchMode.Simple;

    /// <summary>
    /// Gets or sets a value indicating whether the search should be case-sensitive.
    /// </summary>
    /// <remarks>
    /// When false (default), searches are performed case-insensitively for better user experience.
    /// Set to true for precise legal document searches where case matters.
    /// </remarks>
    public bool CaseSensitive { get; set; } = false;

    /// <summary>
    /// Gets or sets search terms for client-related information.
    /// </summary>
    /// <remarks>
    /// Allows searching within matter descriptions for client names, company names, or case references.
    /// Supports comma-separated multiple terms with OR logic.
    /// </remarks>
    [StringLength(256, ErrorMessage = "Client search terms cannot exceed 256 characters.")]
    public string? ClientSearchTerms { get; set; }

    /// <summary>
    /// Gets or sets the matter ID to search for specifically.
    /// </summary>
    /// <remarks>
    /// When provided, searches for a specific matter by ID. Useful for direct matter lookup scenarios.
    /// Takes precedence over other search criteria when specified.
    /// </remarks>
    public Guid? MatterId { get; set; }

    #endregion Enhanced Search Properties

    #region Status and State Filtering

    /// <summary>
    /// Gets or sets a collection of specific matter statuses to include.
    /// </summary>
    /// <remarks>
    /// Allows for more granular status filtering beyond the basic Include flags.
    /// When specified, overrides the individual Include properties.
    /// </remarks>
    public ICollection<MatterStatus>? StatusFilter { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include archived matters in the results.
    /// </summary>
    public bool IncludeArchived { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include deleted matters in the results.
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to include only active matters.
    /// </summary>
    public bool ActiveOnly { get; set; } = false;

    #endregion Status and State Filtering

    #region Enhanced Date Range Properties

    /// <summary>
    /// Gets or sets the earliest creation date to include in the search results.
    /// </summary>
    public DateTime? CreatedAfter { get; set; }

    /// <summary>
    /// Gets or sets the latest creation date to include in the search results.
    /// </summary>
    public DateTime? CreatedBefore { get; set; }

    /// <summary>
    /// Gets or sets the earliest last modified date to include.
    /// </summary>
    /// <remarks>
    /// Filters matters based on when they were last modified (archived, updated, etc.).
    /// Useful for finding recently active matters.
    /// </remarks>
    public DateTime? ModifiedAfter { get; set; }

    /// <summary>
    /// Gets or sets the latest last modified date to include.
    /// </summary>
    public DateTime? ModifiedBefore { get; set; }

    /// <summary>
    /// Gets or sets a predefined date range for common filtering scenarios.
    /// </summary>
    /// <remarks>
    /// Provides convenient date range options like "Last 30 days", "This year", etc.
    /// When specified, overrides individual date properties.
    /// </remarks>
    public DateRangePreset? DateRangePreset { get; set; }

    #endregion Enhanced Date Range Properties

    #region Document and Content Filtering

    /// <summary>
    /// Gets or sets the minimum number of documents a matter must have.
    /// </summary>
    /// <remarks>
    /// Useful for filtering matters by document activity level.
    /// Default null includes all matters regardless of document count.
    /// </remarks>
    [Range(0, int.MaxValue, ErrorMessage = "Minimum document count must be non-negative.")]
    public int? MinDocumentCount { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of documents a matter should have.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Maximum document count must be non-negative.")]
    public int? MaxDocumentCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include only matters with documents.
    /// </summary>
    /// <remarks>
    /// When true, filters out empty matters that have no associated documents.
    /// </remarks>
    public bool HasDocuments { get; set; } = false;

    /// <summary>
    /// Gets or sets document-related search terms.
    /// </summary>
    /// <remarks>
    /// Searches within document file names and descriptions associated with matters.
    /// Useful for finding matters containing specific types of documents.
    /// </remarks>
    [StringLength(256, ErrorMessage = "Document search terms cannot exceed 256 characters.")]
    public string? DocumentSearchTerms { get; set; }

    #endregion Document and Content Filtering

    #region User and Activity Filtering

    /// <summary>
    /// Gets or sets the user ID to filter matters by user activity.
    /// </summary>
    /// <remarks>
    /// Filters matters that have been accessed, modified, or created by a specific user.
    /// Useful for tracking user activity and matter assignments.
    /// </remarks>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Gets or sets the activity type to filter by.
    /// </summary>
    /// <remarks>
    /// Filters matters based on specific activity types like CREATED, VIEWED, MODIFIED, etc.
    /// Combines with UserId for detailed activity-based filtering.
    /// </remarks>
    [StringLength(50, ErrorMessage = "Activity type cannot exceed 50 characters.")]
    public string? ActivityType { get; set; }

    /// <summary>
    /// Gets or sets the minimum activity date for user activity filtering.
    /// </summary>
    public DateTime? ActivityAfter { get; set; }

    #endregion User and Activity Filtering

    #region Pagination and Performance

    /// <summary>
    /// Gets or sets the maximum number of results to return.
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Maximum results must be between 1 and 1000.")]
    public int? MaxResults { get; set; }

    /// <summary>
    /// Gets or sets the sort order for the search results.
    /// </summary>
    public string SortBy { get; set; } = "Description";

    /// <summary>
    /// Gets or sets the sort direction.
    /// </summary>
    public SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    /// <summary>
    /// Gets or sets a value indicating whether to include document count in results.
    /// </summary>
    /// <remarks>
    /// When true, includes document count metadata in search results.
    /// Disable for better performance when document counts are not needed.
    /// </remarks>
    public bool IncludeDocumentCount { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include last activity information.
    /// </summary>
    /// <remarks>
    /// When true, includes last activity timestamp and user information.
    /// Useful for showing recent matter activity but may impact performance.
    /// </remarks>
    public bool IncludeLastActivity { get; set; } = false;

    #endregion Pagination and Performance

    #region Computed Properties

    /// <summary>
    /// Gets a value indicating whether this query has any active search criteria.
    /// </summary>
    [JsonIgnore]
    public bool HasSearchCriteria =>
        !string.IsNullOrWhiteSpace(Description) ||
        !string.IsNullOrWhiteSpace(ClientSearchTerms) ||
        !string.IsNullOrWhiteSpace(DocumentSearchTerms) ||
        MatterId.HasValue ||
        CreatedAfter.HasValue ||
        CreatedBefore.HasValue ||
        ModifiedAfter.HasValue ||
        ModifiedBefore.HasValue ||
        DateRangePreset.HasValue ||
        MinDocumentCount.HasValue ||
        MaxDocumentCount.HasValue ||
        HasDocuments ||
        UserId.HasValue ||
        !string.IsNullOrWhiteSpace(ActivityType) ||
        ActivityAfter.HasValue ||
        ActiveOnly ||
        StatusFilter?.Any() == true;

    /// <summary>
    /// Gets a value indicating whether all date ranges are valid.
    /// </summary>
    [JsonIgnore]
    public bool AreAllDateRangesValid =>
        IsCreatedDateRangeValid && IsModifiedDateRangeValid && IsActivityDateRangeValid;

    /// <summary>
    /// Gets a value indicating whether the created date range is valid.
    /// </summary>
    [JsonIgnore]
    public bool IsCreatedDateRangeValid =>
        !CreatedAfter.HasValue ||
        !CreatedBefore.HasValue ||
        CreatedAfter.Value <= CreatedBefore.Value;

    /// <summary>
    /// Gets a value indicating whether the modified date range is valid.
    /// </summary>
    [JsonIgnore]
    public bool IsModifiedDateRangeValid =>
        !ModifiedAfter.HasValue ||
        !ModifiedBefore.HasValue ||
        ModifiedAfter.Value <= ModifiedBefore.Value;

    /// <summary>
    /// Gets a value indicating whether the activity date range is valid.
    /// </summary>
    [JsonIgnore]
    public bool IsActivityDateRangeValid =>
        !ActivityAfter.HasValue ||
        !CreatedBefore.HasValue ||
        ActivityAfter.Value <= CreatedBefore.Value;

    /// <summary>
    /// Gets a value indicating whether document count range is valid.
    /// </summary>
    [JsonIgnore]
    public bool IsDocumentCountRangeValid =>
        !MinDocumentCount.HasValue ||
        !MaxDocumentCount.HasValue ||
        MinDocumentCount.Value <= MaxDocumentCount.Value;

    /// <summary>
    /// Gets the normalized description for consistent search operations.
    /// </summary>
    [JsonIgnore]
    public string? NormalizedDescription =>
        string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();

    /// <summary>
    /// Gets the effective sort expression combining field and direction.
    /// </summary>
    [JsonIgnore]
    public string EffectiveSortExpression =>
        SortDirection == SortDirection.Descending ? $"{SortBy} desc" : SortBy;

    #endregion Computed Properties

    #region Enhanced Validation

    /// <summary>
    /// Validates the query parameters for logical consistency and business rule compliance.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        // Validate date ranges
        if (!IsCreatedDateRangeValid)
        {
            yield return new ValidationResult(
                "Created After date cannot be later than Created Before date.",
                [nameof(CreatedAfter), nameof(CreatedBefore)]);
        }

        if (!IsModifiedDateRangeValid)
        {
            yield return new ValidationResult(
                "Modified After date cannot be later than Modified Before date.",
                [nameof(ModifiedAfter), nameof(ModifiedBefore)]);
        }

        if (!IsDocumentCountRangeValid)
        {
            yield return new ValidationResult(
                "Minimum document count cannot be greater than maximum document count.",
                [nameof(MinDocumentCount), nameof(MaxDocumentCount)]);
        }

        // Validate date values using MatterValidationHelper
        foreach (var dateValidation in ValidateDates())
        {
            yield return dateValidation;
        }

        // Validate search terms
        foreach (var searchValidation in ValidateSearchTerms())
        {
            yield return searchValidation;
        }

        // Validate business rules
        foreach (var businessValidation in ValidateBusinessRules())
        {
            yield return businessValidation;
        }

        // Validate performance considerations
        foreach (var performanceValidation in ValidatePerformanceConstraints())
        {
            yield return performanceValidation;
        }
    }

    /// <summary>
    /// Validates all date-related properties.
    /// </summary>
    private IEnumerable<ValidationResult> ValidateDates()
    {
        var dates = new[]
        {
            (CreatedAfter, nameof(CreatedAfter)),
            (CreatedBefore, nameof(CreatedBefore)),
            (ModifiedAfter, nameof(ModifiedAfter)),
            (ModifiedBefore, nameof(ModifiedBefore)),
            (ActivityAfter, nameof(ActivityAfter))
        };

        foreach (var (date, propertyName) in dates)
        {
            if (date.HasValue && !MatterValidationHelper.IsValidDate(date.Value))
            {
                yield return new ValidationResult(
                    $"{propertyName} is not within valid date range.",
                    [propertyName]);
            }
        }
    }

    /// <summary>
    /// Validates search term properties.
    /// </summary>
    private IEnumerable<ValidationResult> ValidateSearchTerms()
    {
        // Validate regular expression if using regex mode
        if (SearchMode == SearchMode.Regex && !string.IsNullOrWhiteSpace(Description))
        {
            var regexIsValid = true;
            try
            {
                _ = new System.Text.RegularExpressions.Regex(Description);
            }
            catch (ArgumentException)
            {
                regexIsValid = false;
            }
            if (!regexIsValid)
            {
                yield return new ValidationResult(
                    "Invalid regular expression in Description when SearchMode is Regex.",
                    [nameof(Description)]);
            }
        }

        // Validate wildcard patterns
        if (SearchMode == SearchMode.Wildcard && !string.IsNullOrWhiteSpace(Description) && Description.Contains("***"))
        {
            yield return new ValidationResult(
                "Invalid wildcard pattern: consecutive *** not allowed.",
                [nameof(Description)]);
        }
    }
    /// <summary>
    /// Validates business rule consistency.
    /// </summary>
    private IEnumerable<ValidationResult> ValidateBusinessRules()
    {
        // ActiveOnly implications
        if (ActiveOnly && (IncludeArchived || IncludeDeleted))
        {
            yield return new ValidationResult(
                "When ActiveOnly is true, IncludeArchived and IncludeDeleted should be false.",
                [nameof(ActiveOnly), nameof(IncludeArchived), nameof(IncludeDeleted)]);
        }

        // StatusFilter vs individual flags
        if (StatusFilter?.Any() == true && (IncludeArchived || IncludeDeleted || ActiveOnly))
        {
            yield return new ValidationResult(
                "Cannot use StatusFilter with individual status flags (IncludeArchived, IncludeDeleted, ActiveOnly).",
                [nameof(StatusFilter)]);
        }

        // User activity filtering consistency
        if (!string.IsNullOrWhiteSpace(ActivityType) && !UserId.HasValue)
        {
            yield return new ValidationResult(
                "ActivityType requires UserId to be specified.",
                [nameof(ActivityType), nameof(UserId)]);
        }

        // Date range preset vs individual dates
        if (DateRangePreset.HasValue && (CreatedAfter.HasValue || CreatedBefore.HasValue))
        {
            yield return new ValidationResult(
                "Cannot use DateRangePreset with individual date range properties.",
                [nameof(DateRangePreset)]);
        }
    }

    /// <summary>
    /// Validates performance-related constraints.
    /// </summary>
    private IEnumerable<ValidationResult> ValidatePerformanceConstraints()
    {
        // Warn about potentially expensive operations
        if (SearchMode == SearchMode.Regex && string.IsNullOrWhiteSpace(Description))
        {
            yield return new ValidationResult(
                "Regex search mode requires a description pattern.",
                [nameof(SearchMode), nameof(Description)]);
        }

        if (!string.IsNullOrWhiteSpace(DocumentSearchTerms) && !HasSearchCriteria)
        {
            yield return new ValidationResult(
                "Document search should be combined with other search criteria for better performance.",
                [nameof(DocumentSearchTerms)]);
        }
    }

    #endregion Enhanced Validation

    #region Factory Methods

    /// <summary>
    /// Creates a query optimized for the desktop application's default view.
    /// </summary>
    public static MatterQuery CreateDesktopDefaultQuery()
    {
        return new MatterQuery
        {
            ActiveOnly = true,
            SortBy = "Description",
            SortDirection = SortDirection.Ascending,
            MaxResults = 100,
            IncludeDocumentCount = true,
            IncludeLastActivity = false
        };
    }

    /// <summary>
    /// Creates a high-performance query for large datasets.
    /// </summary>
    public static MatterQuery CreateHighPerformanceQuery()
    {
        return new MatterQuery
        {
            MaxResults = 50,
            IncludeDocumentCount = false,
            IncludeLastActivity = false,
            SortBy = "CreationDate",
            SortDirection = SortDirection.Descending
        };
    }

    /// <summary>
    /// Creates a comprehensive administrative query.
    /// </summary>
    public static MatterQuery CreateAdministrativeQuery()
    {
        return new MatterQuery
        {
            IncludeArchived = true,
            IncludeDeleted = true,
            MaxResults = 200,
            IncludeDocumentCount = true,
            IncludeLastActivity = true,
            SortBy = "ModifiedDate",
            SortDirection = SortDirection.Descending
        };
    }

    /// <summary>
    /// Creates a query for recent matters activity.
    /// </summary>
    public static MatterQuery CreateRecentActivityQuery(int days = 30)
    {
        return new MatterQuery
        {
            DateRangePreset = days switch
            {
                7 => Models.DateRangePreset.Last7Days,
                30 => Models.DateRangePreset.Last30Days,
                90 => Models.DateRangePreset.Last90Days,
                _ => Models.DateRangePreset.Custom
            },
            ModifiedAfter = days == 30 ? null : DateTime.UtcNow.AddDays(-days),
            SortBy = "ModifiedDate",
            SortDirection = SortDirection.Descending,
            IncludeLastActivity = true,
            MaxResults = 100
        };
    }

    #endregion Factory Methods

    #region Utility Methods

    /// <summary>
    /// Applies date range preset logic to the query.
    /// </summary>
    public void ApplyDateRangePreset()
    {
        if (!DateRangePreset.HasValue) return;

        var now = DateTime.UtcNow;
        switch (DateRangePreset.Value)
        {
            case Models.DateRangePreset.Today:
                CreatedAfter = now.Date;
                CreatedBefore = now.Date.AddDays(1).AddTicks(-1);
                break;
            case Models.DateRangePreset.Yesterday:
                var yesterday = now.Date.AddDays(-1);
                CreatedAfter = yesterday;
                CreatedBefore = yesterday.AddDays(1).AddTicks(-1);
                break;
            case Models.DateRangePreset.Last7Days:
                CreatedAfter = now.AddDays(-7);
                CreatedBefore = now;
                break;
            case Models.DateRangePreset.Last30Days:
                CreatedAfter = now.AddDays(-30);
                CreatedBefore = now;
                break;
            case Models.DateRangePreset.Last90Days:
                CreatedAfter = now.AddDays(-90);
                CreatedBefore = now;
                break;
            case Models.DateRangePreset.ThisYear:
                CreatedAfter = new DateTime(now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                CreatedBefore = now;
                break;
            case Models.DateRangePreset.LastYear:
                var lastYear = now.Year - 1;
                CreatedAfter = new DateTime(lastYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                CreatedBefore = new DateTime(lastYear, 12, 31, 23, 59, 59, DateTimeKind.Utc);
                break;
            case Models.DateRangePreset.Custom:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Gets an optimized version of this query for better performance.
    /// </summary>
    public MatterQuery GetOptimizedQuery()
    {
        var optimized = new MatterQuery
        {
            Description = NormalizedDescription,
            SearchMode = SearchMode,
            CaseSensitive = CaseSensitive,
            ClientSearchTerms = ClientSearchTerms?.Trim(),
            MatterId = MatterId,
            StatusFilter = StatusFilter?.ToList(),
            IncludeArchived = IncludeArchived,
            IncludeDeleted = IncludeDeleted,
            ActiveOnly = ActiveOnly,
            CreatedAfter = CreatedAfter,
            CreatedBefore = CreatedBefore,
            ModifiedAfter = ModifiedAfter,
            ModifiedBefore = ModifiedBefore,
            DateRangePreset = DateRangePreset,
            MinDocumentCount = MinDocumentCount,
            MaxDocumentCount = MaxDocumentCount,
            HasDocuments = HasDocuments,
            DocumentSearchTerms = DocumentSearchTerms?.Trim(),
            UserId = UserId,
            ActivityType = ActivityType?.Trim(),
            ActivityAfter = ActivityAfter,
            MaxResults = MaxResults ?? 50, // Default for performance
            SortBy = SortBy,
            SortDirection = SortDirection,
            IncludeDocumentCount = IncludeDocumentCount,
            IncludeLastActivity = IncludeLastActivity
        };

        // Apply optimizations
        optimized.ApplyDateRangePreset();
        optimized.ApplyActiveOnlyLogic();

        return optimized;
    }

    /// <summary>
    /// Applies ActiveOnly logic to the query.
    /// </summary>
    public void ApplyActiveOnlyLogic()
    {
        if (!ActiveOnly) return;
        IncludeArchived = false;
        IncludeDeleted = false;
        StatusFilter = null;
    }

    /// <summary>
    /// Gets a summary of the current query configuration.
    /// </summary>
    public IReadOnlyDictionary<string, object?> GetQuerySummary()
    {
        var summary = new Dictionary<string, object?>
        {
            [nameof(Description)] = NormalizedDescription,
            [nameof(SearchMode)] = SearchMode.ToString(),
            [nameof(CaseSensitive)] = CaseSensitive,
            [nameof(ClientSearchTerms)] = ClientSearchTerms,
            [nameof(MatterId)] = MatterId,
            [nameof(StatusFilter)] = StatusFilter?.Select(s => s.ToString()).ToList(),
            [nameof(ActiveOnly)] = ActiveOnly,
            [nameof(IncludeArchived)] = IncludeArchived,
            [nameof(IncludeDeleted)] = IncludeDeleted,
            [nameof(CreatedAfter)] = CreatedAfter,
            [nameof(CreatedBefore)] = CreatedBefore,
            [nameof(ModifiedAfter)] = ModifiedAfter,
            [nameof(ModifiedBefore)] = ModifiedBefore,
            [nameof(DateRangePreset)] = DateRangePreset?.ToString(),
            [nameof(MinDocumentCount)] = MinDocumentCount,
            [nameof(MaxDocumentCount)] = MaxDocumentCount,
            [nameof(HasDocuments)] = HasDocuments,
            [nameof(DocumentSearchTerms)] = DocumentSearchTerms,
            [nameof(UserId)] = UserId,
            [nameof(ActivityType)] = ActivityType,
            [nameof(ActivityAfter)] = ActivityAfter,
            [nameof(MaxResults)] = MaxResults,
            [nameof(SortBy)] = SortBy,
            [nameof(SortDirection)] = SortDirection.ToString(),
            [nameof(IncludeDocumentCount)] = IncludeDocumentCount,
            [nameof(IncludeLastActivity)] = IncludeLastActivity,
            [nameof(HasSearchCriteria)] = HasSearchCriteria,
            [nameof(AreAllDateRangesValid)] = AreAllDateRangesValid
        };

        return summary;
    }

    #endregion Utility Methods

    #region Equality and Comparison

    /// <summary>
    /// Determines whether the specified MatterQuery is equal to the current MatterQuery.
    /// </summary>
    public bool Equals(MatterQuery? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;

        return Description == other.Description &&
               SearchMode == other.SearchMode &&
               CaseSensitive == other.CaseSensitive &&
               ClientSearchTerms == other.ClientSearchTerms &&
               MatterId == other.MatterId &&
               IncludeArchived == other.IncludeArchived &&
               IncludeDeleted == other.IncludeDeleted &&
               ActiveOnly == other.ActiveOnly &&
               CreatedAfter == other.CreatedAfter &&
               CreatedBefore == other.CreatedBefore &&
               ModifiedAfter == other.ModifiedAfter &&
               ModifiedBefore == other.ModifiedBefore &&
               DateRangePreset == other.DateRangePreset &&
               MaxResults == other.MaxResults &&
               SortBy == other.SortBy &&
               SortDirection == other.SortDirection;
    }

    public override bool Equals(object? obj) => Equals(obj as MatterQuery);

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Description);
        hash.Add(SearchMode);
        hash.Add(CaseSensitive);
        hash.Add(MatterId);
        hash.Add(ActiveOnly);
        hash.Add(MaxResults);
        hash.Add(SortBy);
        hash.Add(SortDirection);
        return hash.ToHashCode();
    }

    #endregion Equality and Comparison

    #region String Representation

    /// <summary>
    /// Returns a detailed string representation of the MatterQuery.
    /// </summary>
    public override string ToString()
    {
        var criteria = new List<string>();

        if (!string.IsNullOrWhiteSpace(NormalizedDescription))
        {
            var modeText = SearchMode == SearchMode.Simple ? "" : $" ({SearchMode})";
            criteria.Add($"Description='{NormalizedDescription}'{modeText}");
        }

        if (MatterId.HasValue)
        {
            criteria.Add($"MatterId={MatterId}");
        }

        if (!string.IsNullOrWhiteSpace(ClientSearchTerms))
        {
            criteria.Add($"Client='{ClientSearchTerms}'");
        }

        if (ActiveOnly)
        {
            criteria.Add("ActiveOnly=True");
        }
        else
        {
            if (IncludeArchived) criteria.Add("IncludeArchived=True");
            if (IncludeDeleted) criteria.Add("IncludeDeleted=True");
        }

        if (StatusFilter?.Any() == true)
        {
            criteria.Add($"Status=[{string.Join(",", StatusFilter)}]");
        }

        if (DateRangePreset.HasValue)
        {
            criteria.Add($"DateRange={DateRangePreset}");
        }
        else
        {
            if (CreatedAfter.HasValue) criteria.Add($"CreatedAfter={CreatedAfter.Value:yyyy-MM-dd}");
            if (CreatedBefore.HasValue) criteria.Add($"CreatedBefore={CreatedBefore.Value:yyyy-MM-dd}");
        }

        if (MinDocumentCount.HasValue) criteria.Add($"MinDocs={MinDocumentCount}");
        if (MaxDocumentCount.HasValue) criteria.Add($"MaxDocs={MaxDocumentCount}");

        if (UserId.HasValue) criteria.Add($"UserId={UserId}");
        if (!string.IsNullOrWhiteSpace(ActivityType)) criteria.Add($"Activity={ActivityType}");

        if (MaxResults.HasValue) criteria.Add($"MaxResults={MaxResults}");

        var sortText = SortDirection == SortDirection.Descending ? $"{SortBy} desc" : SortBy;
        if (!string.Equals(sortText, "Description", StringComparison.OrdinalIgnoreCase))
        {
            criteria.Add($"Sort={sortText}");
        }

        return $"MatterQuery: {(criteria.Any() ? string.Join(", ", criteria) : "No criteria")}";
    }

    #endregion String Representation
}

#region Supporting Enumerations

/// <summary>
/// Defines the available search modes for matter description filtering.
/// </summary>
public enum SearchMode
{
    /// <summary>
    /// Simple contains search (default).
    /// </summary>
    Simple,

    /// <summary>
    /// Exact match search.
    /// </summary>
    Exact,

    /// <summary>
    /// Starts with search.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Ends with search.
    /// </summary>
    EndsWith,

    /// <summary>
    /// Wildcard search (* and ?).
    /// </summary>
    Wildcard,

    /// <summary>
    /// Regular expression search.
    /// </summary>
    Regex
}

/// <summary>
/// Defines the available matter status values for filtering.
/// </summary>
public enum MatterStatus
{
    /// <summary>
    /// Active matter.
    /// </summary>
    Active,

    /// <summary>
    /// Archived matter.
    /// </summary>
    Archived,

    /// <summary>
    /// Deleted matter.
    /// </summary>
    Deleted,

    /// <summary>
    /// All matter statuses.
    /// </summary>
    All
}

/// <summary>
/// Defines the available sort directions.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Ascending sort order.
    /// </summary>
    Ascending,

    /// <summary>
    /// Descending sort order.
    /// </summary>
    Descending
}

/// <summary>
/// Defines predefined date range options for common filtering scenarios.
/// </summary>
public enum DateRangePreset
{
    /// <summary>
    /// Today's matters.
    /// </summary>
    Today,

    /// <summary>
    /// Yesterday's matters.
    /// </summary>
    Yesterday,

    /// <summary>
    /// Last 7 days.
    /// </summary>
    Last7Days,

    /// <summary>
    /// Last 30 days.
    /// </summary>
    Last30Days,

    /// <summary>
    /// Last 90 days.
    /// </summary>
    Last90Days,

    /// <summary>
    /// This year.
    /// </summary>
    ThisYear,

    /// <summary>
    /// Last year.
    /// </summary>
    LastYear,

    /// <summary>
    /// Custom date range (use individual date properties).
    /// </summary>
    Custom
}

#endregion Supporting Enumerations