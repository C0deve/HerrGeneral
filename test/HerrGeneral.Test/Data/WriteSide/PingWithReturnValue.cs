namespace HerrGeneral.Test.Data.WriteSide;

public record PingWithReturnValue : CommandBase
{
    public string Message { get; init; } = string.Empty;

    public class Handler : ILocalCommandHandler<PingWithReturnValue, int>
    {
        public static int? LastValue { get; private set; }

        public MyResult<int> Handle(PingWithReturnValue command)
        {
            LastValue = 1;
            return new MyResult<int>( [new Pong($"{command.Message} received", command.Id, Guid.NewGuid())], 1);

        }
    }
}
