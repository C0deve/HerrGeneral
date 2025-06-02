using System.Collections.ObjectModel;
using System.Reflection;

namespace HerrGeneral.Core.Registration;

internal static class AssemblyExtensions
{
    public static ReadOnlyDictionary<Type, HashSet<Type>> ScanForOpenTypes(this Assembly assembly, HashSet<string> nameSpaces, params HashSet<Type> openTypes) =>
        assembly
            .GetConcreteTypes(nameSpaces)
            .ScanForOpenTypes(openTypes);

    private static ReadOnlyDictionary<Type, HashSet<Type>> ScanForOpenTypes(this IEnumerable<Type> types, IReadOnlyCollection<Type> openTypes)
    {
        var query =
            from type in types
            from openType in openTypes
            where type.IsAssignableFromOpenType(openType)
            group type by openType into g
            select (g.Key, new HashSet<Type>(g));
        
        return query.ToDictionary().AsReadOnly();
    }

    private static IEnumerable<Type> GetConcreteTypes(this Assembly assembly, HashSet<string> nameSpaces) =>
        assembly
            .DefinedTypes
            .Where(type => nameSpaces.Count == 0 || type.IsInNameSpace(nameSpaces))
            .Where(type => type is { IsClass: true, IsAbstract: false });
}