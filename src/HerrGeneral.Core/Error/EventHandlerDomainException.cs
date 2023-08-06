namespace HerrGeneral.Core.Error;

internal class EventHandlerDomainException : DomainException
{
    public EventHandlerDomainException(DomainException innerException) : base(innerException.DomainError, innerException)
    {
    }
}