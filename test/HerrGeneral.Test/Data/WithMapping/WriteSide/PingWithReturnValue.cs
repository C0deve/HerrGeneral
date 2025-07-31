namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

public record PingWithReturnValue : CommandBase
{
    public class Handler : ILocalCommandHandler<PingWithReturnValue, int>
    {
        public MyResult<int> Handle(PingWithReturnValue command) => 
            new( [new Pong(command.Id, Guid.NewGuid())], 1);
    }
}
