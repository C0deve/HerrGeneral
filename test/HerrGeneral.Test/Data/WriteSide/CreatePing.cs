using HerrGeneral.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class CreatePing : CreationCommand
{
    public string Message { get; set; } = string.Empty;

    public class Handler : CreationCommandHandler<CreatePing>
    {
        public Handler(IEventDispatcher eventDispatcher) : base(eventDispatcher)
        {
        }

        public override async Task<CreationResult> Handle(CreatePing command, CancellationToken cancellationToken)
        {
            await Publish(new Pong($"{command.Message} received", command.Id, Guid.NewGuid()), cancellationToken);
            return CreationResult.Success(Guid.NewGuid());
        }

    }
}