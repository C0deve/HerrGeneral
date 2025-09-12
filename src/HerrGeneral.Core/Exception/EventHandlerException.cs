using HerrGeneral.WriteSide;

namespace HerrGeneral.Exception;

/// <summary>
/// Exception wrapper for a panic exception thrown from an <see cref="IEventHandler{TEvent}"/>
/// Used for logging purpose
/// </summary>
/// <param name="exception"></param>
internal class EventHandlerException(System.Exception exception) : System.Exception(exception.Message, exception);