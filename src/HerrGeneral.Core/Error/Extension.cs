using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.Error;

internal static class Extension
{
    /// <summary>
    /// Cast to Domain exception
    /// </summary>
    /// <param name="domainError"></param>
    /// <returns></returns>
    internal static DomainException ToException(this DomainError domainError) => new(domainError);
}