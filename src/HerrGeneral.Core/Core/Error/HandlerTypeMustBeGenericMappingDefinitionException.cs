namespace HerrGeneral.Core.Error;

/// <summary>
/// The given handler type is not generic
/// </summary>
public class HandlerTypeMustBeGenericMappingDefinitionException(Type handlerType) : Exception($"Handler type is not generic. '{handlerType}'");