namespace ADMS.API.Models;

/// <summary>
///     Link model
/// </summary>
/// <remarks>
///     Constructor
/// </remarks>
/// <param name="href">URL</param>
/// <param name="rel">relationship</param>
/// <param name="method">method</param>
public class LinkDto(string? href, string? rel, string method)
{
    /// <summary>
    ///     URL to return
    /// </summary>
    public string? Href { get; private set; } = href;

    /// <summary>
    ///     Relationship
    /// </summary>
    public string? Rel { get; private set; } = rel;

    /// <summary>
    ///     Method
    /// </summary>
    public string Method { get; private set; } = method;
}