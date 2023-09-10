using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public record CreatePing : Create
{
    public static readonly Guid AggregateId  = Guid.NewGuid();

    public string Message { get; set; } = string.Empty;

    public class Handler : CreateHandler<CreatePing>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<CreateResult> Handle(CreatePing command, CancellationToken cancellationToken)
        {
            await Publish(new Pong($"{command.Message} received", command.Id, AggregateId), cancellationToken);
            return CreateResult.Success(AggregateId);
        }

    }
}