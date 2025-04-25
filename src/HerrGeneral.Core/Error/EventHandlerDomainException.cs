namespace HerrGeneral.Core.Error;

internal class EventHandlerDomainException(Exception innerException) :
    DomainException(innerException);