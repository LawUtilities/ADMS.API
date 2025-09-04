namespace ADMS.API.ResourceParameters;

/// <summary>
/// Resource Parameters for Revisions API
/// </summary>
public class RevisionsResourceParameters : PagedResourceParameters
{
    public bool IncludeDeleted { get; set; } = false;

    public RevisionsResourceParameters()
    {
        OrderBy = "CreatedAt"; // or whatever default is appropriate
    }
}
