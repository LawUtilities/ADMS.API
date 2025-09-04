namespace ADMS.API.ResourceParameters;

/// <summary>
/// Resource parameters for querying and paginating document audit records.
/// </summary>
public class DocumentAuditsResourceParameters : PagedResourceParameters
{
    /// <summary>
    /// Gets or sets the search query to filter document audits by relevant fields.
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Gets or sets the order by clause for sorting document audits.
    /// Inherited from <see cref="PagedResourceParameters"/>.
    /// </summary>
    public new string? OrderBy
    {
        get => base.OrderBy;
        set => base.OrderBy = value;
    }

    // Additional filtering properties can be added here as needed.
}