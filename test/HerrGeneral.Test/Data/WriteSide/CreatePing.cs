namespace HerrGeneral.Test.Data.WriteSide;

public record CreatePing : CommandBase
{
    public required Guid AggregateId  { get; init; }
    public required string Message { get; init; }
    
    public class Handler : ILocalCommandHandler<CreatePing, Guid>
    {
        public MyResult<Guid> Handle(CreatePing command) =>
            new ([new Pong($"{command.Message} received", command.Id, command.AggregateId)],
                command.AggregateId);
    }

}

