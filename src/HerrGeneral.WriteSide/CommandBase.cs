using System.Text;

namespace HerrGeneral.WriteSide;

/// <summary>
/// Command implementation
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract class CommandBase<TResult> : ICommand<TResult> where TResult : IWithSuccess
{
    /// <summary>
    /// Id of the command
    /// </summary>
    public Guid Id { get; }
    
    /// <summary>
    /// Execution date of the command
    /// </summary>
    public DateTime ExecutionDate { get; }
    

    /// <summary>
    /// Create a command object
    /// </summary>
    protected CommandBase() : this(DateTime.Now) {}
    
    /// <summary>
    /// Create a command object
    /// </summary>
    /// <param name="executionDate"></param>
    protected CommandBase(DateTime executionDate)
    {
        Id = Guid.NewGuid();
        ExecutionDate = executionDate;
    }

    /// <summary>
    /// Log the command
    /// </summary>
    /// <param name="sb"></param>
    /// <returns></returns>
    public virtual StringBuilder Log(StringBuilder sb) =>
        sb.AppendLine($"-- Execution date {ExecutionDate:g}");
}