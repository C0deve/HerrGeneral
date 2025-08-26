namespace HerrGeneral.Core;

/// <summary>
/// Interface of the unit of work.
/// One unit of per command handler
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Starts the transaction associated with unit of work
    /// </summary>
    void Start();

    /// <summary>
    /// Commits the transaction associated with unit of work
    /// </summary>
    void Commit();

    /// <summary>
    /// Rolls back the transaction associated with unit of work
    /// </summary>
    void RollBack();

}