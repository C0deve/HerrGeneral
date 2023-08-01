using System.Collections.ObjectModel;

namespace HerrGeneral;

public static class Extensions
{
    public static bool IsEmpty(this Guid guid) => guid == Guid.Empty;
    public static TReturn WithValue<TReturn>(this Guid guid, Func<Guid, TReturn> onValue)
    {
        if (guid.IsEmpty())
            throw new ArgumentNullException(nameof(guid));

        return onValue(guid);
    }
    public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
    
    public static string GetFriendlyName(this Type type)
    {
        var friendlyName = type.Name;
        if (!type.IsGenericType) return friendlyName;

        var iBacktick = friendlyName.IndexOf('`');

        if (iBacktick > 0) friendlyName = friendlyName.Remove(iBacktick);

        friendlyName += "<";
        var typeParameters = type.GetGenericArguments();
        for (var i = 0; i < typeParameters.Length; ++i)
        {
            var typeParamName = GetFriendlyName(typeParameters[i]);
            friendlyName += (i == 0 ? typeParamName : "," + typeParamName);
        }

        friendlyName += ">";

        return friendlyName;
    }
    public static string ToLog(this Exception e) =>
        $"!! Panic exception of type {e.GetType().GetFriendlyName()}\n-- Message : {e.Message}\n-- StackTrace :\n{e.StackTrace}\n";

    public static void AddRange<T>(this HashSet<T> source, IEnumerable<T> values)
    {
        foreach (var value in values) source.Add(value);
    }

    public static ReadOnlyDictionary<TKey, TValue> AsReadonly<TKey, TValue>(this Dictionary<TKey, TValue> source) 
        where TKey : notnull => new(source);
}