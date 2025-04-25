namespace HerrGeneral.Core.Error;

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