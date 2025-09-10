using System.Collections.ObjectModel;
using System.Reflection;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Specify an assembly and an optional list of namespace
/// </summary>
/// <param name="Assembly"></param>
/// <param name="Namespaces"></param>
internal record ScanParam(Assembly Assembly, HashSet<string> Namespaces)
{
    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="Assembly"></param>
    /// <param name="Namespaces"></param>
    public ScanParam(Assembly Assembly, params string[] Namespaces) : this(Assembly, new HashSet<string>(Namespaces))
    {
    }
}