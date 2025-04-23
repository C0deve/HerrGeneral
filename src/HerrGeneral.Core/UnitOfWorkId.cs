namespace HerrGeneral.Core;

/// <summary>
/// Identifier of a unit of work.
/// A unit of work is triggered by a command.
/// </summary>
public record UnitOfWorkId
{
    /// <summary>
    /// id value
    /// </summary>
    public Guid Value { get; }

    /// <summary>
    /// Return a new id
    /// </summary>
    /// <returns></returns>
    public static UnitOfWorkId New() => new();

    private UnitOfWorkId() => Value = Guid.NewGuid();

    /// <summary>
    /// Implicit convert from UnitOfWorkId to Guid
    /// </summary>
    /// <param name="uow"></param>
    /// <returns></returns>
    public static implicit operator Guid(UnitOfWorkId uow) => uow.Value;

    /// <summary>
    /// Implicit convert from Guid to UnitOfWorkId
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static implicit operator UnitOfWorkId(Guid id) =>
        id.WithValue(x => new UnitOfWorkId(x));
}