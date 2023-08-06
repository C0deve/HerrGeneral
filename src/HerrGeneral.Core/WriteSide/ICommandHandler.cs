using HerrGeneral.Contracts.WriteSide;

namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// Interface for command handler
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResult"></typeparam>
public interface ICommandHandler<in TCommand, TResult>
    where TCommand: ICommand<TResult>
{
    /// <summary>
    /// Handle the command
    /// </summary>
    /// <param name="command"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<TResult> Handle(TCommand command, CancellationToken cancellationToken);
}