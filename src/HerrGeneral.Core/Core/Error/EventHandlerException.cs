using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.Error;

/// <summary>
/// Exception wrapper for a panic exception thrown from an <see cref="IEventHandler{TEvent}"/>
/// Used for logging purpose
/// </summary>
/// <param name="exception"></param>
internal class EventHandlerException(Exception exception) : Exception(exception.Message, exception);