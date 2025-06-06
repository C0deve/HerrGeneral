using HerrGeneral.Core.ReadSide;
using HerrGeneral.Core.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Extension methods for registration
/// </summary>
public static class ServiceExtension
{
    /// <summary>
    /// Configure HerrGeneral :
    /// Scan and register all command handlers, write side event handlers and read side event handlers 
    /// Add required services 
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="scannerDelegate"></param>
    /// <returns></returns>
    public static IServiceCollection UseHerrGeneral(
        this IServiceCollection serviceCollection,
        Func<Scanner, Scanner> scannerDelegate
    )
    {
        var scanner = scannerDelegate(new Scanner()).Scan();

        foreach (var commandHandler in scanner.CommandHandlerWithReturnTypes)
            serviceCollection.RegisterOpenType(commandHandler, Scanner.CommandHandlerInterfaceReturnType, ServiceLifetime.Transient);
        foreach (var eventHandler in scanner.EventHandlerTypes)
            serviceCollection.RegisterOpenType(eventHandler, Scanner.WriteSideEventHandlerInterface, ServiceLifetime.Transient);
        foreach (var eventHandler in scanner.ReadSideEventHandlerTypes)
            serviceCollection.RegisterOpenType(eventHandler, Scanner.ReadSideEventHandlerInterface, ServiceLifetime.Singleton);
        serviceCollection.AddSingleton<CommandLogger>();
        serviceCollection.AddSingleton<ReadSideEventDispatcher>();
        serviceCollection.AddSingleton<IAddEventToDispatch>(x => x.GetRequiredService<ReadSideEventDispatcher>());
        serviceCollection.AddSingleton<WriteSideEventDispatcher>();
        serviceCollection.AddSingleton<Mediator>();

        return serviceCollection;
    }

    private static void RegisterOpenType(this IServiceCollection serviceCollection, Type tHandler, Type openTypeInterface, ServiceLifetime serviceLifetime)
    {
        if (serviceLifetime == ServiceLifetime.Singleton)
        {
            serviceCollection.AddSingleton(tHandler);
            serviceCollection.RegisterOpenTypeInternal(tHandler, openTypeInterface, ServiceLifetime.Transient);
        }
        else
            serviceCollection.RegisterOpenTypeInternal(tHandler, openTypeInterface, serviceLifetime);
    }

    private static void RegisterOpenTypeInternal(this IServiceCollection serviceCollection, Type tHandler, Type openTypeInterface, ServiceLifetime serviceLifetime)
    {
        foreach (var @interface in tHandler.GetCloseInterfacesFromOpenInterface(openTypeInterface))
            serviceCollection.Add(new ServiceDescriptor(
                @interface,
                provider => provider.GetRequiredService(tHandler),
                serviceLifetime));
    }
}