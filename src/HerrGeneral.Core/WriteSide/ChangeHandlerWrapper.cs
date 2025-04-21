using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

internal class ChangeHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, ChangeResult> where TCommand : Change
{
    public override Task<ChangeResult> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        WithExceptionToCommandResult(BuildPipeline<Unit>(serviceProvider))((TCommand)command, cancellationToken);

    private static HandlerWrapperDelegate<TCommand, ChangeResult> WithExceptionToCommandResult(
        CommandPipeline.HandlerDelegate<TCommand, Unit> next) =>
        (command, cancellationToken) =>
            Task.Run(() =>
            {
                try
                {
                    var _ = next(command, cancellationToken);
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