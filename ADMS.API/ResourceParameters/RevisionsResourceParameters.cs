namespace ADMS.API.ResourceParameters;

/// <summary>
/// Resource parameters for querying and paginating revision records.
/// </summary>
public class RevisionsResourceParameters : PagedResourceParameters
{
    /// <summary>
    /// Gets or sets a value indicating whether to include deleted revisions in the results.
    /// </summary>
    public bool IncludeDeleted { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of the <see cref="RevisionsResourceParameters"/> class
    /// with default sorting by RevisionNumber.
    /// </summary>
    public RevisionsResourceParameters()
    {
        OrderBy = "RevisionNumber";
    }

    /// <summary>
    /// Gets or sets the order by clause for sorting revisions.
    /// Inherited from <see cref="PagedResourceParameters"/>.
    /// </summary>
    public new string? OrderBy
    {
        get => base.OrderBy;
        set => base.OrderBy = value;
    }

    // Additional filtering or search properties can be added here as needed.
}