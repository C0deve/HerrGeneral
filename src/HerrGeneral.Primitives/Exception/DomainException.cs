namespace HerrGeneral.Exception;

/// <summary>
/// Exception wrapper for a domain error
/// </summary>
public class DomainException : System.Exception
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="innerDomainException"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public DomainException(System.Exception innerDomainException) : base($"{innerDomainException.GetType()} : {innerDomainException.Message}", innerDomainException)
    {
    }
}
