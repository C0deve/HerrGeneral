using HerrGeneral.Core.Registration.Policy;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Provides methods to retrieve write-side and read-side registration policies
/// for a given configuration, including mapped handlers.
/// </summary>
internal class RegistrationPolicyProvider
{
    
    /// <summary>
    /// Gets all write-side policies including mapped handlers
    /// </summary>
    /// <param name="configuration">The configuration containing mappings</param>
    /// <returns>Array of write-side policies</returns>
    public virtual IRegistrationPolicy[] GetWriteSidePolicies(Configuration configuration) =>
    [
        new RegisterICommandHandler(),
        new RegisterWriteSideEventHandler(),
        new RegisterMappedCommandHandlers(configuration.CommandHandlerMappings),
        new RegisterMappedWriteSideEventHandlers(configuration.WriteSideEventHandlerMappings)
    ];

    /// <summary>
    /// Gets all read-side policies including mapped handlers
    /// </summary>
    /// <param name="configuration">The configuration containing mappings</param>
    /// <returns>Array of read-side policies</returns>
    public virtual IRegistrationPolicy[] GetReadSidePolicies(Configuration configuration) =>
    [
        new RegisterReadSideEventHandler(),
        new RegisterMappedReadSideEventHandlers(configuration.ReadSideEventHandlerMappings)
    ];
}