namespace HerrGeneral.Error;

public class DomainException : Exception
{
    public DomainError DomainError { get; }

    protected DomainException(DomainError domainError, DomainException domainException): base($"{domainError.GetType()} : {domainError.Message}", domainException) => 
        DomainError = domainError ?? throw new ArgumentNullException(nameof(domainError));
    
    public DomainException(DomainError domainError): base($"{domainError.GetType()} : {domainError.Message}") => 
        DomainError = domainError ?? throw new ArgumentNullException(nameof(domainError));

    public string ToLog() => 
        $"!! DomainException of type {DomainError.GetType().GetFriendlyName()}\n-- Message : {DomainError.Message}\n-- StackTrace :\n{StackTrace}\n";
}