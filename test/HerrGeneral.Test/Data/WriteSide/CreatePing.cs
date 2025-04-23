using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record CreatePing : CommandBase
{
    public required Guid AggregateId  { get; init; }
    public required string Message { get; init; }
    
    public class Handler : CommandHandler<CreatePing, Guid>
    {
        public override (IEnumerable<object> Events, Guid Result) Handle(CreatePing command, CancellationToken cancellationToken) =>
            ([new Pong($"{command.Message} received", command.Id, command.AggregateId)],
                command.AggregateId);
    }

}

