using HerrGeneral.WriteSide;

namespace HerrGeneral.DDD.Core;

internal static class TypeExtensions
{
    extension(Type commandType)
    {
        public Type MakeHandlerInterfaceForCreateCommand<TResult>() =>
            typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResult));

        public Type MakeCreateHandlerInternalType(Type aggregateType, Type dynamicHandler) =>
            typeof(CreateHandlerInternal<,,>).MakeGenericType(aggregateType, commandType, dynamicHandler);

        public Type MakeDynamicCreateHandlerType(Type aggregateType) =>
            typeof(CreateHandlerByReflection<,>).MakeGenericType(aggregateType, commandType);

        public Type MakeHandlerInterfaceForChangeCommand() =>
            typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(Unit));

        public Type MakeChangeHandlerType(Type aggregateType, Type dynamicHandler) =>
            typeof(ChangeHandlerInternal<,,>).MakeGenericType(aggregateType, commandType, dynamicHandler);

        public Type MakeDynamicChangeHandlerType(Type aggregateType) =>
            typeof(ChangeHandlerByReflection<,>).MakeGenericType(aggregateType, commandType);

        public Type GetAggregateTypeFromCommand() =>
            commandType.BaseType!.GetGenericArguments()[0];
    }
}