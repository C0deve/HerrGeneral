using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Define all inner type used by HerrGeneral
/// </summary>
internal static class TypeDefinition
{
    /// <summary>
    /// Command handler interface
    /// </summary>
    public static readonly Type CommandHandlerInterface = typeof(ICommandHandler<,>);

    /// <summary>
    /// Event handler interface from write side 
    /// </summary>
    public static readonly Type WriteSideEventHandlerInterface = typeof(IEventHandler<>);

    /// <summary>
    /// Event handler interface from read side
    /// </summary>
    public static readonly Type ReadSideEventHandlerInterface = typeof(HerrGeneral.ReadSide.IEventHandler<>);

}