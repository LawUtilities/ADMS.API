namespace ADMS.API.Services;

/// <summary>
/// Constructor
/// </summary>
/// <param name="destinationProperties"></param>
/// <param name="revert"></param>
/// <exception cref="ArgumentNullException"></exception>
public class PropertyMappingValue(IEnumerable<string> destinationProperties,
        bool revert = false)
{
    /// <summary>
    /// Destination Properties
    /// </summary>
    public IEnumerable<string> DestinationProperties { get; private set; } = destinationProperties
            ?? throw new ArgumentNullException(nameof(destinationProperties));

    /// <summary>
    /// Revert boolean
    /// </summary>
    public bool Revert { get; private set; } = revert;
}