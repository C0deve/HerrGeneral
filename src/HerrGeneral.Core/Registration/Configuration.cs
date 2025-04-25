using System.Reflection;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Configuration for registration and for handling commands and events
/// </summary>
public class Configuration
{
    private readonly HashSet<ScanParam> _writeSideSearchParams = [];
    private readonly HashSet<ScanParam> _readSideSearchParams = [];
    private readonly HashSet<Type> _domainExceptionInterfaces = [];

    /// <summary>
    /// Write side assemblies to scan
    /// </summary>
    public IEnumerable<ScanParam> WriteSideSearchParams => _writeSideSearchParams.AsEnumerable();

    /// <summary>
    /// Read side assemblies to scan
    /// </summary>
    public IEnumerable<ScanParam> ReadSideSearchParams => _readSideSearchParams.AsEnumerable();

    /// <summary>
    /// List of domain exception types
    /// </summary>
    public IReadOnlySet<Type> DomainExceptionTypes => _domainExceptionInterfaces;

    internal Configuration()
    {
    }

    /// <summary>
    /// Add an assembly to scan for read side
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="namespaces"></param>
    /// <returns></returns>
    public Configuration UseReadSideAssembly(Assembly assembly, params string[] namespaces)
    {
        _readSideSearchParams.Add(new ScanParam(assembly, namespaces));
        return this;
    }

    /// <summary>
    ///  Add an assembly to scan for write side
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="namespaces"></param>
    /// <returns></returns>
    public Configuration UseWriteSideAssembly(Assembly assembly, params string[] namespaces)
    {
        _writeSideSearchParams.Add(new ScanParam(assembly, namespaces));
        return this;
    }

    /// <summary>
    /// Set interface(s) used to catch domain exception
    /// </summary>
    public Configuration UseDomainException<TException>() where TException : Exception
    {
        _domainExceptionInterfaces.Add(typeof(TException));
        return this;
    }

    /// <summary>
    /// Valid the configuration
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    internal void ThrowIfNotValid()
    {
        if (_writeSideSearchParams.Count == 0 && _readSideSearchParams.Count == 0)
            throw new InvalidOperationException($"No assembly. Use {nameof(UseWriteSideAssembly)} or {nameof(UseReadSideAssembly)} to specify on which assemblies to scan");
    }
}