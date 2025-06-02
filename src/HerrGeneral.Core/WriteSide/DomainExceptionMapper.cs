using HerrGeneral.Core.Error;

namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// Map external exception to internal <see cref="DomainException"/>
/// </summary>
/// <param name="mappings"></param>
internal class DomainExceptionMapper(params Type[] mappings)
{
    public Exception Map(Exception exception) => 
        mappings.Any(e => exception.GetType().IsAssignableTo(e)) 
            ? new DomainException(exception) 
            : exception;

    public Exception Map(Exception exception, 
        Func<Exception, Exception> onDomainException,
        Func<Exception, Exception> onPanicException)=> 
        mappings.Any(e => exception.GetType().IsAssignableTo(e)) 
            ? onDomainException(exception) 
            : onPanicException(exception);
}