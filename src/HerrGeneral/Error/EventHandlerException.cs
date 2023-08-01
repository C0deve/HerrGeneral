namespace HerrGeneral.Error;

internal class EventHandlerException : Exception
{
    public EventHandlerException(Exception exception) : base(exception.Message, exception) { }
}