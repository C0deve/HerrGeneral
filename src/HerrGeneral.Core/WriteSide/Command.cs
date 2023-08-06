namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// A command returning a CommandResultV2
/// </summary>
public abstract class Command : CommandBase<CommandResultV2>
{
    /// <summary>
    /// Ctor
    /// </summary>
    protected Command()
    {
        
    }
    
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="executionDate"></param>
    protected Command(DateTime executionDate) : base(executionDate)
    {
    }
}