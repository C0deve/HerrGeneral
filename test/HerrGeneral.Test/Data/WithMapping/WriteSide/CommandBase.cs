namespace HerrGeneral.Test.Data.WithMapping.WriteSide;

/// <summary>
/// Command implementation
/// </summary>
public abstract record CommandBase
{
    /// <summary>
    /// Id of the command
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();
}