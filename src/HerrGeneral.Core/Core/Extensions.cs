using System.Text;

namespace HerrGeneral.Core;

internal static class Extensions
{
    private static readonly ConcurrentDictionary<Type, string> FriendlyNameCache = new();

    /// <summary>
    /// Display the type with a friendly name
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetFriendlyName(this Type type) =>
        FriendlyNameCache.GetOrAdd(type, BuildFriendlyName);

    private static string BuildFriendlyName(Type type)
    {
        if (!type.IsGenericType) return type.Name;

        var name = type.Name;
        var iBacktick = name.IndexOf('`');
        var baseName = iBacktick > 0 ? name[..iBacktick] : name;

        var typeParameters = type.GetGenericArguments();
        var sb = new StringBuilder(baseName).Append('<');
        for (var i = 0; i < typeParameters.Length; ++i)
        {
            if (i > 0) sb.Append(',');
            sb.Append(typeParameters[i].GetFriendlyName());
        }
        sb.Append('>');

        return sb.ToString();
    }
}