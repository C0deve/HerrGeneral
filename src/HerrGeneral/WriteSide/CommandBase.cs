using System.Text;
using HerrGeneral.Contracts;

namespace HerrGeneral.WriteSide;

public abstract class CommandBase<TResult> : ICommand<TResult> where TResult : IWithSuccess
{
    public Guid Id { get; }
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

    public virtual StringBuilder Log(StringBuilder sb) =>
        sb.AppendLine($"-- Execution date {ExecutionDate:g}");
}