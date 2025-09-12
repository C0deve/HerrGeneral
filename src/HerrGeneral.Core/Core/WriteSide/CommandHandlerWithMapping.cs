using System.Reflection;
using HerrGeneral.Core.Error;
using HerrGeneral.Core.ReadSide;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// Internal handler used to map client handler
/// </summary>
/// <param name="handler"></param>
/// <param name="mappingProvider"></param>
/// <typeparam name="TCommand"></typeparam>
/// <typeparam name="THandler"></typeparam>
/// <typeparam name="TResult"></typeparam>
internal class CommandHandlerWithMapping<TCommand, THandler, TResult>(THandler handler, CommandHandlerMappings mappingProvider)
    : ICommandHandler<TCommand, TResult>, IHandlerTypeProvider
    where TCommand : notnull
    where THandler : notnull
{
    public (IEnumerable<object> Events, TResult Result) Handle(TCommand command)
    {
        var mapping = mappingProvider.GetFromCommand(command, typeof(TResult));

        var handleMethod =
            typeof(THandler).GetMethod(mapping.MethodInfo.Name) ?? throw new InvalidOperationException();

        try
        {
            var result = handleMethod.Invoke(handler, [command]) ?? throw new InvalidOperationException();

            try
            {
                var events = mapping.MapEvents(result);
                dynamic value =
                    mapping.MapValue is null
                        ? Unit.Default
                        : Convert.ChangeType(mapping.MapValue(result), typeof(TResult));

                return (events, value);
            }
            catch (Exception e)
            {
                var mappingHandlerType = mapping.MethodInfo.DeclaringType!;
                throw new ConversionException(result.GetType(), mappingHandlerType, e);
            }
        }
        // throw only the innerException of TargetInvocationException produce by handleMethod.Invoke. 
        catch (TargetInvocationException e)
        {
            throw e.InnerException ?? e;
        }
    }

    public Type GetHandlerType() => typeof(THandler);
}