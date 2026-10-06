namespace HerrGeneral.Exception;

/// <summary>
/// Exception wrapper for a domain error
/// </summary>
/// <remarks>
/// Ctor
/// </remarks>
/// <param name="innerDomainException"></param>
/// <exception cref="ArgumentNullException"></exception>
public class DomainException(System.Exception innerDomainException) : System.Exception($"{innerDomainException.GetType()} : {innerDomainException.Message}", innerDomainException)
{
}
