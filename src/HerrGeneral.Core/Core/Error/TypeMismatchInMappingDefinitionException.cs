using System.Reflection;

namespace HerrGeneral.Core.Error;

/// <summary>
/// TReturn does not match the return type of the method that handles TCommand 
/// AddMapping{TCommand, THandler, TReturn}
/// </summary>
public class TypeMismatchInMappingDefinitionException(Type expectingReturnType, MethodInfo givenMethod)
    : Exception($"Return type of '{givenMethod.DeclaringType?.GetFriendlyName()}.{givenMethod.Name}(...)' doesn't match provided return type (TReturn)\n" +
                $"Actual: '{givenMethod.ReturnType.GetFriendlyName()}', provided: '{expectingReturnType.GetFriendlyName()}'.");