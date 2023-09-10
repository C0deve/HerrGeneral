namespace HerrGeneral.WriteSide;

/// <summary>
/// A command returning a CommandResultV2
/// </summary>
public abstract record Change : CommandBase<ChangeResult>
{
    /// <summary>
    /// Ctor
    /// </summary>
    protected Change()
    {
        
    }
    
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="executionDate"></param>
    protected Change(DateTime executionDate) : base(executionDate)
    {
    }
}