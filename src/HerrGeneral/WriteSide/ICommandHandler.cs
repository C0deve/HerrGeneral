using HerrGeneral.Contracts;

namespace HerrGeneral.WriteSide;

public interface ICommandHandler<in TCommand, TResult>
    where TCommand: ICommand<TResult>
{
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}