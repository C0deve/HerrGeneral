namespace HerrGeneral.Core.Registration;

internal static class TypeExtensions
{
    public static bool IsInNameSpace(this Type type, ICollection<string> nameSpaces) =>
        type.Namespace is not null && nameSpaces.Contains(type.Namespace);

    public static bool IsAssignableFromOpenType(this Type type, Type openType)
    {
        var isAssignable = type.GetInterfaces().Any(t =>
            t.IsGenericType &&
            t.GetGenericTypeDefinition() == openType);

        if (isAssignable)
            return true;

        return type.BaseType != null && type.BaseType != typeof(object) && IsAssignableFromOpenType(type.BaseType, openType);
    }

    public static IEnumerable<Type> GetCloseInterfacesFromOpenInterface(this Type type, Type openTypeInterface)
    {
        var closeInterfaces = type
            .GetInterfaces()
            .Where(t =>
                t is { IsInterface: true, IsGenericType: true } &&
                t.GetGenericTypeDefinition() == openTypeInterface)
            .ToList();

        if (closeInterfaces.Any()) 
            return closeInterfaces;
        
        if (type.BaseType == null || type.BaseType == typeof(object))
            return [];

        return GetCloseInterfacesFromOpenInterface(type.BaseType, openTypeInterface);
    }
}