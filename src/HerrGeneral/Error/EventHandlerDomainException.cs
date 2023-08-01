namespace HerrGeneral.Error;

internal class EventHandlerDomainException : DomainException
{
    public EventHandlerDomainException(DomainException innerException) : base(innerException.DomainError, innerException)
    {
    }
}