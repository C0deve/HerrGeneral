using System.Reflection;

namespace HerrGeneral.Core.Registration;

internal static class TypeExtensions
{
    public static bool IsInNameSpace(this Type type, ICollection<string> nameSpaces) =>
        type.Namespace is not null && nameSpaces.Contains(type.Namespace);

    public static bool IsAssignableFromOpenType(this Type type, Type openType)
    {
        if(type.IsGenericType && type.GetGenericTypeDefinition() == openType) 
            return true;
        
        var isAssignable = type.GetInterfaces().Any(t =>
            t.IsGenericType &&
            t.GetGenericTypeDefinition() == openType);

        if (isAssignable)
            return true;

        return type.BaseType != null && type.BaseType != typeof(object) && IsAssignableFromOpenType(type.BaseType, openType);
    }

    public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
    {
        var interfaceTypes = givenType.GetInterfaces();

        if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
            return true;

        if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
            return true;

        var baseType = givenType.BaseType;
        return baseType != null && IsAssignableToGenericType(baseType, genericType);
    }
    
    /// <summary>
    /// Return all interfaces with a generic type definition equals to <paramref name="openTypeInterface"/> 
    /// </summary>
    /// <param name="type"></param>
    /// <param name="openTypeInterface"></param>
    /// <returns></returns>
    public static IEnumerable<Type> GetInterfacesHavingGenericOpenType(this Type type, Type openTypeInterface)
    {
        var closeInterfaces = type
            .GetInterfaces()
            .Where(t =>
                t is { IsInterface: true, IsGenericType: true } &&
                t.GetGenericTypeDefinition() == openTypeInterface)
            .ToList();

        if (closeInterfaces.Count != 0)
            return closeInterfaces;

        if (type.BaseType == null || type.BaseType == typeof(object))
            return [];

        return GetInterfacesHavingGenericOpenType(type.BaseType, openTypeInterface);
    }

    public static MethodInfo? FindMethodWithUniqueParameterOfType(this Type type, Type uniqueParameterType) =>
        new[] { type }
            .Union(type.GetInterfaces())
            .Select(t => t.GetMethods().FirstOrDefault(info => info.HasUniqueParameterOfType(uniqueParameterType)))
            .OfType<MethodInfo>()
            .FirstOrDefault();

    internal static bool HasUniqueParameterOfType(this MethodInfo methodInfo, Type uniqueParameter)
    {
        var parameters = methodInfo.GetParameters();
        return parameters.Length == 1 && parameters[0].ParameterType == uniqueParameter;
    }
}