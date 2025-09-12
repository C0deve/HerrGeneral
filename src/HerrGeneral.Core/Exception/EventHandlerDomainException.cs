using HerrGeneral.WriteSide;

namespace HerrGeneral.Exception;

/// <summary>
/// Exception wrapper for a domain error thrown from an <see cref="IEventHandler{TEvent}"/>
/// Used for logging purpose
/// </summary>
/// <param name="innerException"></param>
internal class EventHandlerDomainException(System.Exception innerException) :
    DomainException(innerException);