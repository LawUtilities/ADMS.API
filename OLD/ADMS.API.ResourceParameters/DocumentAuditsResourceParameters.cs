namespace ADMS.API.ResourceParameters;

public class DocumentAuditsResourceParameters : PagedResourceParameters
{
    // Example: Add filtering/sorting properties as needed
    public string? SearchQuery { get; set; }
    public string? OrderBy { get; set; }
}
