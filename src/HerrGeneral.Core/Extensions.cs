using System.Collections.ObjectModel;

namespace HerrGeneral.Core;

/// <summary>
/// Extensions methods
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Fluent guid validation
    /// </summary>
    /// <param name="guid"></param>
    /// <returns></returns>
    public static bool IsEmpty(this Guid guid) => guid == Guid.Empty;

    /// <summary>
    /// Fluent string validation
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty(this string s) => string.IsNullOrEmpty(s);
    
    /// <summary>
    /// Display the type with a friendly name
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Add multiple items to the hashSet 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="values"></param>
    /// <typeparam name="T"></typeparam>
    public static void AddRange<T>(this HashSet<T> source, IEnumerable<T> values)
    {
        foreach (var value in values) source.Add(value);
    }

    /// <summary>
    /// Cast a Dictionary to a ReadOnlyDictionary
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public static ReadOnlyDictionary<TKey, TValue> AsReadonly<TKey, TValue>(this Dictionary<TKey, TValue> source) 
        where TKey : notnull => new(source);
}