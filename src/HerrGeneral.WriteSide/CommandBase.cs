namespace HerrGeneral.WriteSide;

/// <summary>
/// Command implementation
/// </summary>
/// <typeparam name="TResult"></typeparam>
public abstract class CommandBase<TResult>
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
}