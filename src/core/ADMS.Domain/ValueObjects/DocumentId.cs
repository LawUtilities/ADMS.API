namespace ADMS.Domain.ValueObjects;

/// <summary>
/// Represents a unique identifier for a document in the ADMS system.
/// </summary>
public sealed record DocumentId
{
    public Guid Value { get; }

    private DocumentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Document ID cannot be empty", nameof(value));
        Value = value;
    }

    /// <summary>
    /// Creates a new DocumentId with a generated GUID.
    /// </summary>
    public static DocumentId New() => new(Guid.NewGuid());

    /// <summary>
    /// Creates a DocumentId from an existing GUID value.
    /// </summary>
    public static DocumentId From(Guid value) => new(value);

    /// <summary>
    /// Creates a DocumentId from a string representation of a GUID.
    /// </summary>
    public static DocumentId From(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException($"Invalid GUID format: {value}", nameof(value));
        return new DocumentId(guid);
    }

    public static implicit operator Guid(DocumentId id) => id.Value;
    public static implicit operator DocumentId(Guid value) => From(value);

    public override string ToString() => Value.ToString();
}
