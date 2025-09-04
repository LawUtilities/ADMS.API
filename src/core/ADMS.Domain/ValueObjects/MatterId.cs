namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for a matter in the ADMS system.
/// </summary>
public sealed record MatterId
{
    public Guid Value { get; }

    private MatterId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Matter ID cannot be empty", nameof(value));
        Value = value;
    }

    /// <summary>
    /// Creates a new MatterId with a generated GUID.
    /// </summary>
    public static MatterId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a MatterId from an existing GUID value.
    /// </summary>
    public static MatterId From(Guid value) => new(value);

    /// <summary>
    /// Creates a MatterId from a string representation of a GUID.
    /// </summary>
    public static MatterId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid GUID format: {value}", nameof(value));
        return new MatterId(guid);
    }

    public static implicit operator Guid(MatterId id) => id.Value;
    public static implicit operator MatterId(Guid value) => From(value);

    public override string ToString() => Value.ToString();
}