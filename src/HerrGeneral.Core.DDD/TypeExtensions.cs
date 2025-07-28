using HerrGeneral.WriteSide;
using HerrGeneral.WriteSide.DDD;

namespace HerrGeneral.Core.DDD;

internal static class TypeExtensions
{
    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        // First case: direct check - is this type itself a generic with the matching definition?
        if (givenType.IsDirectlyAssignableToGenericType(genericType))
            return true;

        // Second case: interface check - using Any for short-circuit evaluation
        if (givenType
            .GetInterfaces()
            .Any(it => it.IsDirectlyAssignableToGenericType(genericType)))
            return true;

        // Third case: recursive check on base type
        var baseType = givenType.BaseType;
        if (baseType == null || baseType == typeof(object))
            return false;
        
        return IsAssignableToGenericType(baseType, genericType);
    }

    private static bool IsDirectlyAssignableToGenericType(this Type givenType, Type genericType) => 
        givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType;

    public static Type MakeHandlerInterfaceForCreateCommand<TResult>(this Type commandType) =>
        typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

    public static Type MakeCreateHandlerInternalType(this Type commandType, Type aggregateType, Type dynamicHandler) =>
        typeof(CreateHandlerInternal<,,>).MakeGenericType(aggregateType, commandType, dynamicHandler);

    public static Type MakeDynamicCreateHandlerType(this Type commandType, Type aggregateType) =>
        typeof(CreateHandlerByReflection<,>).MakeGenericType(aggregateType, commandType);

    public static bool IsChangeCommand(this Type type) =>
        type.BaseType?.IsGenericType == true &&
        type.BaseType.GetGenericTypeDefinition() == typeof(Change<>);

    public static Type MakeHandlerInterfaceForChangeCommand(this Type commandType) =>
        typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(Unit));

    public static Type MakeChangeHandlerType(this Type commandType, Type aggregateType, Type dynamicHandler) =>
        typeof(ChangeHandlerInternal<,,>).MakeGenericType(aggregateType, commandType, dynamicHandler);

    public static Type MakeDynamicChangeHandlerType(this Type commandType, Type aggregateType) =>
        typeof(ChangeHandlerByReflection<,>).MakeGenericType(aggregateType, commandType);

    public static Type GetAggregateTypeFromCommand(this Type commandType) =>
        commandType.BaseType!.GetGenericArguments()[0];
}