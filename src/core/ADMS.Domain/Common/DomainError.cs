namespace ADMS.Domain.Common;

/// <summary>
/// Represents a domain-specific error with code and description.
/// </summary>
public sealed record DomainError(string Code, string Description)
{
    public static readonly DomainError None = new(string.Empty, string.Empty);

    public static implicit operator string(DomainError error) => error.Code;
}