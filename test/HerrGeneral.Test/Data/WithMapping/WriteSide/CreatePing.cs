namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record CreatePing : CommandBase
{
    public required Guid AggregateId  { get; init; }
    public required string Message { get; init; }
    
    public class Handler : ILocalCommandHandler<CreatePing, Guid>
    {
        public MyResult<Guid> Handle(CreatePing command) =>
            new ([new Pong(command.Id, command.AggregateId)],
                command.AggregateId);
    }

}

