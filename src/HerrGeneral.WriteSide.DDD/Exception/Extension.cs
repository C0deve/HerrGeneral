namespace HerrGeneral.WriteSide.DDD.Exception;

internal static class Extension
{
    /// <summary>
    /// Cast to Domain exception
    /// </summary>
    /// <param name="domainError"></param>
    /// <returns></returns>
    internal static DomainException ToDomainException(this DomainError domainError) => new(domainError);
}