namespace HerrGeneral.Core.Error;

/// <summary>
/// Exception wrapper for a domain error
/// </summary>
internal class DomainException : Exception
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="innerDomainException"></param>
    /// <exception cref="ArgumentNullException"></exception>
    internal DomainException(Exception innerDomainException): base($"{innerDomainException.GetType()} : {innerDomainException.Message}", innerDomainException)
    {
    }
}