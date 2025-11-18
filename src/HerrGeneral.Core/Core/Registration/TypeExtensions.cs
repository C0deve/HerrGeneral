using System.Reflection;

namespace HerrGeneral.Core.Registration;

internal static class TypeExtensions
{
    /// <param name="type"></param>
    extension(Type type)
    {
        public bool IsInNameSpace(ICollection<string> nameSpaces) =>
            type.Namespace is not null && nameSpaces.Any(nameSpace => type.Namespace.StartsWith(nameSpace));

        public bool IsAssignableFromOpenType(Type openType)
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

        /// <summary>
        /// Return all interfaces with a generic type definition equals to <paramref name="openTypeInterface"/> 
        /// </summary>
        /// <param name="openTypeInterface"></param>
        /// <returns></returns>
        public IEnumerable<Type> GetInterfacesHavingGenericOpenType(Type openTypeInterface)
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

        public MethodInfo? FindMethodWithUniqueParameterOfType(Type uniqueParameterType) =>
            new[] { type }
                .Union(type.GetInterfaces())
                .Select(t => t.GetMethods().FirstOrDefault(info => info.HasUniqueParameterOfType(uniqueParameterType)))
                .OfType<MethodInfo>()
                .FirstOrDefault();
    }

    internal static bool HasUniqueParameterOfType(this MethodInfo methodInfo, Type uniqueParameter)
    {
        var parameters = methodInfo.GetParameters();
        return parameters.Length == 1 && parameters[0].ParameterType == uniqueParameter;
    }
}