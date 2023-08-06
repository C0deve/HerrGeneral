using System.Collections.ObjectModel;
using System.Reflection;

namespace HerrGeneral.Core.Registration;

internal static class AssemblyExtensions
{
    public static ReadOnlyDictionary<Type, HashSet<Type>> ScanForOpenTypes(this Assembly assembly, HashSet<string> nameSpaces, HashSet<Type> openTypes) =>
        assembly
            .GetConcreteTypes(nameSpaces)
            .ScanForOpenTypes(openTypes);

    private static ReadOnlyDictionary<Type, HashSet<Type>> ScanForOpenTypes(this IEnumerable<Type> types, IReadOnlyCollection<Type> openTypes)
    {
        var result = openTypes.ToDictionary(
            type => type,
            _ => new HashSet<Type>());

        foreach (var type in types)
        {
            foreach (var openType in openTypes.Where(openType => type.IsAssignableFromOpenType(openType)))
            {
                result[openType].Add(type);
                break;
            }
        }

        return new ReadOnlyDictionary<Type, HashSet<Type>>(result);
    }

    private static IEnumerable<Type> GetConcreteTypes(this Assembly assembly, ICollection<string> nameSpaces) =>
        assembly
            .GetTypes()
            .Where(type => nameSpaces.Count == 0 || type.IsInNameSpace(nameSpaces))
            .Where(type => type is { IsClass: true, IsAbstract: false });
}