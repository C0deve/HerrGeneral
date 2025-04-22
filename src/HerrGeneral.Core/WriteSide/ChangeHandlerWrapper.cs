using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

internal class ChangeHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, ChangeResult>
{
    public override Task<ChangeResult> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        WithExceptionToCommandResult(BuildPipeline<Unit>(serviceProvider))(Guid.NewGuid(), (TCommand)command, cancellationToken);

    private static HandlerWrapperDelegate<TCommand, ChangeResult> WithExceptionToCommandResult(
        CommandPipeline.HandlerDelegate<TCommand, Unit> next) =>
        (operationId, command, cancellationToken) =>
            Task.Run(() =>
            {
                try
                {
                    var _ = next(operationId, command, cancellationToken);
                    return ChangeResult.Success;
                }
                catch (EventHandlerDomainException domainException)
                {
                    return ChangeResult.DomainFail(domainException.DomainError);
                }
                catch (DomainException domainException)
                {
                    return ChangeResult.DomainFail(domainException.DomainError);
                }
                catch (EventHandlerException e)
                {
                    return ChangeResult.PanicFail(e);
                }
                catch (Exception e)
                {
                    return ChangeResult.PanicFail(e);
                }
            }, cancellationToken);
}