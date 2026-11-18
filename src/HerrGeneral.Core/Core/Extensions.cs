using System.Text;

namespace HerrGeneral.Core;

internal static class Extensions
{
    private static readonly ConcurrentDictionary<Type, string> FriendlyNameCache = new();

    /// <summary>
    /// Display the type with a friendly name: generic arguments are written out and a nested type is prefixed
    /// by its declaring types ("CreateOrder.Command", "Order.Handler&lt;Guid&gt;"), so that types sharing a
    /// short name ("Command", "Handler", "Service") can be told apart in traces and messages.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static string GetFriendlyName(this Type type) =>
        FriendlyNameCache.GetOrAdd(type, BuildFriendlyName);

    private static string BuildFriendlyName(Type type)
    {
        if (type.IsGenericParameter) return type.Name;

        // A nested type receives the generic arguments of its declaring types first, then its own.
        var genericArguments = type.IsGenericType ? type.GetGenericArguments() : [];
        var argumentIndex = 0;

        var declaringChain = new Stack<Type>();
        for (var current = type; current is not null; current = current.DeclaringType)
            declaringChain.Push(current);

        var sb = new StringBuilder();
        foreach (var current in declaringChain)
        {
            if (sb.Length > 0) sb.Append('.');

            var name = current.Name;
            var iBacktick = name.IndexOf('`');
            if (iBacktick < 0)
            {
                sb.Append(name);
                continue;
            }

            sb.Append(name, 0, iBacktick).Append('<');
            var arity = int.Parse(name.AsSpan(iBacktick + 1));
            for (var i = 0; i < arity; ++i)
            {
                if (i > 0) sb.Append(',');
                sb.Append(argumentIndex < genericArguments.Length
                    ? genericArguments[argumentIndex++].GetFriendlyName()
                    : "?");
            }
            sb.Append('>');
        }

        return sb.ToString();
    }
}
