namespace HerrGeneral.Test.Data.WriteSide;

/// <summary>
/// Command implementation
/// </summary>
public abstract record CommandBase
{
    /// <summary>
    /// Id of the command
    /// </summary>
    protected Guid Id { get; } = Guid.NewGuid();
}