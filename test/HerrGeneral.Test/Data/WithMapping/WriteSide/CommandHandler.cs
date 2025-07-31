using HerrGeneral.Core;

namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

/// <summary>
/// Handler for command returning a CommandResult
/// </summary>
/// <typeparam name="TCommand"></typeparam>
public abstract class CommandHandler<TCommand> : ILocalCommandHandler<TCommand> where TCommand : CommandBase
{
    /// <summary>
    /// Ctor
    /// </summary>
    protected CommandHandler()
    {
    }

    /// <summary>
    /// Handle a command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    public MyResult<Unit> Handle(TCommand command)
        => new(InnerHandle(command), Unit.Default);

    /// <summary>
    /// Handle a command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    protected abstract IEnumerable<object> InnerHandle(TCommand command);
}