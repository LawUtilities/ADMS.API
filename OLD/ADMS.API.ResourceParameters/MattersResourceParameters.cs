namespace ADMS.API.ResourceParameters;

/// <summary>
/// Resource Parameters for Matters API
/// </summary>
public class MattersResourceParameters : PagedResourceParameters
{
    /// <summary>
    /// Matter description.
    /// </summary>
    public string Description { get; set; } = String.Empty;

    /// <summary>
    /// Is the matter archived?
    /// </summary>
    public bool IsArchived { get; set; } = false;

    /// <summary>
    /// Is the matter deleted?
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// MatterResourceParameters constructor.
    /// </summary>
    public MattersResourceParameters()
    {
        OrderBy = "CreatedAt"; // or whatever default is appropriate
    }
}
