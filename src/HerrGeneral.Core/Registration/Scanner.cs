namespace HerrGeneral.Core.Registration;

/// <summary>
/// Scan close types assignable from an open type definition
/// </summary>
internal static class Scanner
{
    /// <summary>
    /// Scan on the provided ScanParams for concrete types corresponding to one of the provided open type  
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public static Dictionary<Type, HashSet<Type>> Scan(IEnumerable<ScanParam> scanParams, HashSet<Type> openTypesToScan) =>
        (
            from scanParam in scanParams
            from scanResult in scanParam.Scan(openTypesToScan)
            group scanResult by scanResult.Key
            into g
            let y =
                from valuePair in g
                from t in valuePair.Value
                orderby t.FullName
                select t
            select (g.Key, y.ToHashSet())
        )
        .ToDictionary();
}