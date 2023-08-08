namespace HerrGeneral.WriteSide;

/// <summary>
/// Command for aggregate creation.
/// Return a CreationResult containing the id of the created aggregate.
/// </summary>
public abstract class CreationCommand : CommandBase<CreationResult>
{
    /// <summary>
    /// Ctor
    /// </summary>
    protected CreationCommand()
    {
        
    }
    
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="executionDate"></param>
    protected CreationCommand(DateTime executionDate) : base(executionDate)
    {
    }
}