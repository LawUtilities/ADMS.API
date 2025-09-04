namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for a document revision in the ADMS system.
/// </summary>
public sealed record RevisionId
{
    public Guid Value { get; }

    private RevisionId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Revision ID cannot be empty", nameof(value));
        Value = value;
    }

    /// <summary>
    /// Creates a new RevisionId with a generated GUID.
    /// </summary>
    public static RevisionId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a RevisionId from an existing GUID value.
    /// </summary>
    public static RevisionId From(Guid value) => new(value);

    /// <summary>
    /// Creates a RevisionId from a string representation of a GUID.
    /// </summary>
    public static RevisionId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid GUID format: {value}", nameof(value));
        return new RevisionId(guid);
    }

    public static implicit operator Guid(RevisionId id) => id.Value;
    public static implicit operator RevisionId(Guid value) => From(value);

    public override string ToString() => Value.ToString();
}