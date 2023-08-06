using System.Text;

namespace HerrGeneral.WriteSide;

/// <summary>
/// Command interface (Write side)
/// </summary>
/// <typeparam name="TResult"></typeparam>
public interface ICommand<out TResult>
{
    /// <summary>
    /// Id of the command
    /// </summary>
    Guid Id { get; }
    
    /// <summary>
    /// Execution date of the command
    /// </summary>
    DateTime ExecutionDate { get; }

    /// <summary>
    /// Used to control log format of the command
    /// </summary>
    /// <param name="sb"></param>
    /// <returns></returns>
    StringBuilder Log(StringBuilder sb);
}