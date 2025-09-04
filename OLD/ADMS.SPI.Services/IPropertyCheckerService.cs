namespace ADMS.API.Services;

/// <summary>
/// Property Checker Service interfaxe
/// </summary>
public interface IPropertyCheckerService
{
    /// <summary>
    /// Verifies that the type has specified properties
    /// </summary>
    /// <typeparam name="T">datatype to verify</typeparam>
    /// <param name="fields">fields to verify</param>
    /// <returns>True if properties exist, False otherwise</returns>
    bool TypeHasProperties<T>(string? fields);
}
