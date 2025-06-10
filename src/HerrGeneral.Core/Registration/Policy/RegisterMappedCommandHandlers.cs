using HerrGeneral.Core.WriteSide;
using HerrGeneral.WriteSide;
using Microsoft.Extensions.DependencyInjection;

namespace HerrGeneral.Core.Registration.Policy;

/// <summary>
/// Registers all external command handlers present in the mappings
/// </summary>
/// <param name="commandHandlerMappings"></param>
internal class RegisterMappedCommandHandlers(CommandHandlerMappings commandHandlerMappings) : IRegistrationPolicy
{
    public HashSet<Type> GetOpenTypes() =>
        commandHandlerMappings
            .All()
            .Select(mapping => mapping.HandlerGenericType)
            .ToHashSet();

    public void Register(IServiceCollection serviceCollection, Dictionary<Type, HashSet<Type>> externalHandlers)
    {
        var scanResults =
            from mapping in commandHandlerMappings.All()
            from externalHandler in externalHandlers[mapping.HandlerGenericType]
            let commandType = externalHandler
                .GetMethod(mapping.MethodInfo.Name)!
                .GetParameters()[0]
                .ParameterType
            select new CommandHandlerScanResult(commandType, externalHandler, mapping.ReturnValueType);

        foreach (var scanResult in scanResults)
        {
            var @interface = typeof(ICommandHandler<,>).MakeGenericType(scanResult.CommandType, scanResult.ValueType);
            var internalHandler = typeof(CommandHandlerWithMapping<,,>).MakeGenericType(scanResult.CommandType, scanResult.HandlerType, scanResult.ValueType);

            serviceCollection.Add(new ServiceDescriptor(
                @interface,
                internalHandler,
                ServiceLifetime.Transient));
        }
    }

    private record CommandHandlerScanResult(Type CommandType, Type HandlerType, Type ValueType);
}