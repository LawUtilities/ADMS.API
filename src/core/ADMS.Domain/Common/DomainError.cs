namespace ADMS.Domain.Common;

public sealed record DomainError(string Code, string Description)
{
    public static readonly DomainError None = new(string.Empty, string.Empty);

    public static implicit operator string(DomainError error) => error.Code;
}