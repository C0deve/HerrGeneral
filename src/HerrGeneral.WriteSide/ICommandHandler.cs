namespace HerrGeneral.WriteSide;

/// <summary>
/// Interface for command handler
/// </summary>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="TResult"></typeparam>
public interface ICommandHandler<in TCommand, TResult>
{
    /// <summary>
    /// Handle the command
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    (IEnumerable<object> Events, TResult Result) Handle(TCommand command);
}