namespace HerrGeneral.DDD;

/// <summary>
/// Command implementation
/// </summary>
public abstract record CommandBase
{
    /// <summary>
    /// Id of the command
    /// </summary>
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// Execution date of the command
    /// </summary>
    public DateTime ExecutionDate { get; init; } = DateTime.Now;
}