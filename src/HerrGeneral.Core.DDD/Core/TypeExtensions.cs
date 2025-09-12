using HerrGeneral.WriteSide;

namespace HerrGeneral.DDD.Core;

internal static class TypeExtensions
{
    public static Type MakeHandlerInterfaceForCreateCommand<TResult>(this Type commandType) =>
        typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

    public static Type MakeCreateHandlerInternalType(this Type commandType, Type aggregateType, Type dynamicHandler) =>
        typeof(CreateHandlerInternal<,,>).MakeGenericType(aggregateType, commandType, dynamicHandler);

    public static Type MakeDynamicCreateHandlerType(this Type commandType, Type aggregateType) =>
        typeof(CreateHandlerByReflection<,>).MakeGenericType(aggregateType, commandType);

    public static Type MakeHandlerInterfaceForChangeCommand(this Type commandType) =>
        typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(Unit));

    public static Type MakeChangeHandlerType(this Type commandType, Type aggregateType, Type dynamicHandler) =>
        typeof(ChangeHandlerInternal<,,>).MakeGenericType(aggregateType, commandType, dynamicHandler);

    public static Type MakeDynamicChangeHandlerType(this Type commandType, Type aggregateType) =>
        typeof(ChangeHandlerByReflection<,>).MakeGenericType(aggregateType, commandType);

    public static Type GetAggregateTypeFromCommand(this Type commandType) =>
        commandType.BaseType!.GetGenericArguments()[0];
}