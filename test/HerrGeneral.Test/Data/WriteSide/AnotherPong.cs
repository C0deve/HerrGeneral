namespace HerrGeneral.Test.Data.WriteSide;

public record AnotherPong : EventBase
{
    public string Message { get; }

    public AnotherPong(string message, Guid sourceCommandId, Guid aggregateId) : base(sourceCommandId, aggregateId) => 
        Message = message;
}