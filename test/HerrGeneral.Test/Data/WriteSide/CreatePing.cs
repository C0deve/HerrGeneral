using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record CreatePing : Create
{
    public static readonly Guid AggregateId = Guid.NewGuid();

    public string Message { get; set; } = string.Empty;
    
    public class Handler() : CreateHandler<CreatePing>
    {
        public override (IEnumerable<object> Events, Guid Result) Handle(CreatePing command, CancellationToken cancellationToken) =>
            ([new Pong($"{command.Message} received", command.Id, AggregateId)],
                AggregateId);
    }
}

