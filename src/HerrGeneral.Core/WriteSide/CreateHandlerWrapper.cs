using HerrGeneral.Core.Error;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

internal class CreateHandlerWrapper<TCommand> : CommandHandlerWrapperBase<TCommand, CreateResult>
    where TCommand : Create
{
    public override Task<CreateResult> Handle(object command, IServiceProvider serviceProvider, CancellationToken cancellationToken) =>
        WithExceptionToCommandResult(BuildPipeline<Guid>(serviceProvider))(Guid.NewGuid(), (TCommand)command, cancellationToken);

    private static HandlerWrapperDelegate<TCommand, CreateResult> WithExceptionToCommandResult(
        CommandPipeline.HandlerDelegate<TCommand, Guid> next) =>
        (operationId, command, cancellationToken) =>
            Task.Run(() =>
            {
                try
                {
                    var result = next(operationId, command, cancellationToken);
                    return CreateResult.Success(result.Result);
                }
                catch (EventHandlerDomainException domainException)
                {
                    return CreateResult.DomainFail(domainException.DomainError);
                }
                catch (DomainException domainException)
                {
                    return CreateResult.DomainFail(domainException.DomainError);
                }
                catch (EventHandlerException e)
                {
                    return CreateResult.PanicFail(e);
                }
                catch (Exception e)
                {
                    return CreateResult.PanicFail(e);
                }
            }, cancellationToken);
}