namespace DDDPlayground.Domain.Exceptions;

/// <summary>
/// Base exception for domain-related errors.
/// Represents business rule violations.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
