namespace HerrGeneral;

/// <summary>
/// Interface of the unit of work.
/// One unit of per command handler
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Starts the transaction associated with command(sourceCommandId)
    /// </summary>
    /// <param name="sourceCommandId">Unique id of the command</param>
    void Start(Guid sourceCommandId);
        
    /// <summary>
    /// Commits the transaction associated with command(sourceCommandId)
    /// </summary>
    /// <param name="sourceCommandId">Unique id of the command</param>
    void Commit(Guid sourceCommandId);

    /// <summary>
    /// RollBack the transaction associated with command(sourceCommandId)
    /// </summary>
    /// <param name="sourceCommandId">Unique id of the command</param>
    void RollBack(Guid sourceCommandId);
        
    /// <summary>
    /// RollBack the transaction associated with command(sourceCommandId)
    /// </summary>
    /// <param name="sourceCommandId">Unique id of the command</param>
    void Dispose(Guid sourceCommandId);
}