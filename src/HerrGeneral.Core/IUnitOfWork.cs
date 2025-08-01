namespace HerrGeneral.Core;

/// <summary>
/// Interface of the unit of work.
/// One unit of per command handler
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Starts the transaction associated with unit of work
    /// </summary>
    /// <param name="unitOfWorkId">Unique id of the unit of work</param>
    void Start(UnitOfWorkId unitOfWorkId);
    
    /// <summary>
    /// Commits the transaction associated with unit of work
    /// </summary>
    /// <param name="unitOfWorkId">Unique id of the unit of work</param>
    void Commit(UnitOfWorkId unitOfWorkId);
    
    /// <summary>
    /// Rolls back the transaction associated with unit of work
    /// </summary>
    /// <param name="unitOfWorkId">Unique id of the unit of work</param>
    void RollBack(UnitOfWorkId unitOfWorkId);
    
    /// <summary>
    /// Disposes the transaction associated with unit of work
    /// </summary>
    /// <param name="unitOfWorkId">Unique id of the unit of work</param>
    void Dispose(UnitOfWorkId unitOfWorkId);
}