namespace ADMS.Domain.Common;

public sealed class DomainException : Exception
{
    public DomainError Error { get; }

    public DomainException(DomainError error) : base(error.Description)
    {
        Error = error;
    }

    public DomainException(string message) : base(message)
    {
        Error = new DomainError("DOMAIN_ERROR", message);
    }
}