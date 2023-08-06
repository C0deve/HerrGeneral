using HerrGeneral.Core.WriteSide;

namespace HerrGeneral.Test.Data.WriteSide;

public class PongMiddleHandler : IEventHandler<Pong>
{
    private readonly Dependency _dependency;

    public PongMiddleHandler(Dependency dependency)
    {
        _dependency = dependency;
    }

    public Task Handle(Pong notification, CancellationToken cancellationToken)
    {
        _dependency.Called = true;
        return Task.CompletedTask;
    }
}