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
    public static readonly Type ReadSideEventHandlerInterface = typeof(HerrGeneral.ReadSide.IProjectionEventHandler<>);

    /// <summary>
    /// Post-transaction projection handler interface
    /// </summary>
    public static readonly Type PostProjectionHandlerInterface = typeof(IHandlePostProjection<>);

    /// <summary>
    /// In-transaction synchronous projection handler interface
    /// </summary>
    public static readonly Type SyncProjectionHandlerInterface = typeof(IHandleSyncProjection<>);

    /// <summary>
    /// Post-transaction side effect handler interface
    /// </summary>
    public static readonly Type SideEffectHandlerInterface = typeof(IHandleSideEffect<>);

}