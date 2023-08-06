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

    public static IEnumerable<Type> GetCloseTypesFromOpenType(this Type type, Type openTypeInterface)
    {
        var closeTypes = type
            .GetInterfaces()
            .Where(t =>
                t.IsGenericType &&
                t.GetGenericTypeDefinition() == openTypeInterface);

        foreach (var closeType in closeTypes)
            yield return closeType;

        if (type.BaseType == null || type.BaseType == typeof(object))
            yield break;

        foreach (var parentCloseType in GetCloseTypesFromOpenType(type.BaseType, openTypeInterface))
            yield return parentCloseType;
    }
}