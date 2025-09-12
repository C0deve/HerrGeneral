namespace HerrGeneral.Exception;

/// <summary>
/// The given handler type is not generic
/// </summary>
public class HandlerTypeMustBeGenericMappingDefinitionException(Type handlerType) : System.Exception($"Handler type is not generic. '{handlerType}'");