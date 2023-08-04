namespace HerrGeneral.Error;

/// <summary>
/// Exception wrapper for a domain error
/// </summary>
public class DomainException : Exception
{
    /// <summary>
    /// The domain error
    /// </summary>
    public DomainError DomainError { get; }

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="domainError"></param>
    /// <param name="innerDomainException"></param>
    /// <exception cref="ArgumentNullException"></exception>
    protected DomainException(DomainError domainError, DomainException innerDomainException): base($"{domainError.GetType()} : {domainError.Message}", innerDomainException) => 
        DomainError = domainError ?? throw new ArgumentNullException(nameof(domainError));
    
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="domainError"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public DomainException(DomainError domainError): base($"{domainError.GetType()} : {domainError.Message}") => 
        DomainError = domainError ?? throw new ArgumentNullException(nameof(domainError));
}