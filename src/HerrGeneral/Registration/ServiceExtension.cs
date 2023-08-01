using HerrGeneral.ReadSide.Dispatcher;
using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.Dispatcher;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Registration;

public static class ServiceExtension
{
    public static IServiceCollection UseHerrGeneral(
        this IServiceCollection serviceCollection,
        Func<Scanner, Scanner> scannerDelegate
    )
    {
        var scanner = scannerDelegate(new Scanner()).Scan();

        foreach (var commandHandler in scanner.CommandHandlerTypes)
            serviceCollection.RegisterOpenType(commandHandler, typeof(ICommandHandler<,>), ServiceLifetime.Transient);

        foreach (var eventHandler in scanner.EventHandlerTypes)
            serviceCollection.RegisterOpenType(eventHandler, typeof(IEventHandler<>), ServiceLifetime.Transient);

        foreach (var eventHandler in scanner.ReadSideEventHandlerTypes)
            serviceCollection.RegisterOpenType(eventHandler, typeof(ReadSide.Contracts.IEventHandler<>), ServiceLifetime.Singleton);

        serviceCollection.AddSingleton<ReadSideEventDispatcher>();
        serviceCollection.AddSingleton<IAddEventToDispatch>(x => x.GetRequiredService<ReadSideEventDispatcher>());
        serviceCollection.AddSingleton<ReadSide.IEventDispatcher>(x => x.GetRequiredService<ReadSideEventDispatcher>());

        serviceCollection.AddSingleton<IEventDispatcher, EventDispatcher>();
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
        foreach (var @interface in tHandler.GetCloseTypesFromOpenType(openTypeInterface))
            serviceCollection.Add(new ServiceDescriptor(
                @interface,
                provider => provider.GetRequiredService(tHandler),
                serviceLifetime));
    }
}