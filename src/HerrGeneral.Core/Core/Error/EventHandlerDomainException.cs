using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.Error;

/// <summary>
/// Exception wrapper for a domain error thrown from an <see cref="IEventHandler{TEvent}"/>
/// Used for logging purpose
/// </summary>
/// <param name="innerException"></param>
internal class EventHandlerDomainException(Exception innerException) :
    DomainException(innerException);