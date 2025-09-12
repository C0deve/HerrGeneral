using HerrGeneral.Exception;

namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// Map external exception to internal <see cref="DomainException"/>
/// </summary>
/// <param name="mappings"></param>
internal class DomainExceptionMapper(params Type[] mappings)
{
    public System.Exception Map(System.Exception exception) => 
        mappings.Any(e => exception.GetType().IsAssignableTo(e)) 
            ? new DomainException(exception) 
            : exception;

    public System.Exception Map(System.Exception exception, 
        Func<System.Exception, System.Exception> onDomainException,
        Func<System.Exception, System.Exception> onPanicException)=> 
        mappings.Any(e => exception.GetType().IsAssignableTo(e)) 
            ? onDomainException(exception) 
            : onPanicException(exception);
}