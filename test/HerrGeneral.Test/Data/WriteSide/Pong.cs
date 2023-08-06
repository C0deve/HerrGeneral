namespace HerrGeneral.Test.Data.WriteSide;

public class Pong : EventBase
{
    public string Message { get; }

    public Pong(string message, Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId) => 
        Message = message;
}