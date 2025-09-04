namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for a user in the ADMS system.
/// </summary>
public sealed record UserId
{
    public Guid Value { get; }

    private UserId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(value));
        Value = value;
    }

    /// <summary>
    /// Creates a new UserId with a generated GUID.
    /// </summary>
    public static UserId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a UserId from an existing GUID value.
    /// </summary>
    public static UserId From(Guid value) => new(value);

    /// <summary>
    /// Creates a UserId from a string representation of a GUID.
    /// </summary>
    public static UserId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid GUID format: {value}", nameof(value));
        return new UserId(guid);
    }

    public static implicit operator Guid(UserId id) => id.Value;
    public static implicit operator UserId(Guid value) => From(value);

    public override string ToString() => Value.ToString();
}
