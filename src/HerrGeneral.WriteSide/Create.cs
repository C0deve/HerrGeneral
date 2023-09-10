namespace HerrGeneral.WriteSide;

/// <summary>
/// Command for aggregate creation.
/// Return a CreationResult containing the id of the created aggregate.
/// </summary>
public abstract record Create : CommandBase<CreateResult>
{
    /// <summary>
    /// Ctor
    /// </summary>
    protected Create()
    {
        
    }
    
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="executionDate"></param>
    protected Create(DateTime executionDate) : base(executionDate)
    {
    }
}