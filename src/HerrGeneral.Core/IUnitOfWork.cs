namespace HerrGeneral.Core;

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
    void Start(UnitOfWorkId sourceCommandId);

    /// <summary>
    /// Commits the transaction associated with command(sourceCommandId)
    /// </summary>
    /// <param name="sourceCommandId">Unique id of the command</param>
    void Commit(UnitOfWorkId sourceCommandId);

    /// <summary>
    /// RollBack the transaction associated with command(sourceCommandId)
    /// </summary>
    /// <param name="sourceCommandId">Unique id of the command</param>
    void RollBack(UnitOfWorkId sourceCommandId);

    /// <summary>
    /// RollBack the transaction associated with command(sourceCommandId)
    /// </summary>
    /// <param name="sourceCommandId">Unique id of the command</param>
    void Dispose(UnitOfWorkId sourceCommandId);
}