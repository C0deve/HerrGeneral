using System.Reflection;
using HerrGeneral.Core.WriteSide;

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
    internal IEnumerable<ScanParam> WriteSideSearchParams => _writeSideSearchParams.AsEnumerable();

    /// <summary>
    /// Read side assemblies to scan
    /// </summary>
    internal IEnumerable<ScanParam> ReadSideSearchParams => _readSideSearchParams.AsEnumerable();

    /// <summary>
    /// List of domain exception types
    /// </summary>
    internal IReadOnlySet<Type> DomainExceptionTypes => _domainExceptionInterfaces;

    /// <summary>
    /// List of mapping of external handlers
    /// </summary>
    internal CommandHandlerMappings CommandHandlerMappings { get; } = new();
    
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
            throw new InvalidOperationException($"No assembly to scan. Use {nameof(UseWriteSideAssembly)} or {nameof(UseReadSideAssembly)} to specify on which assemblies to scan");
    }
    
    /// <summary>
    /// Map an external command handler returning events
    /// </summary>
    /// <param name="mapEvents"></param>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    /// <typeparam name="TReturn"></typeparam>
    /// <returns></returns>
    public Configuration MapCommandHandler<TCommand, THandler, TReturn>(Func<TReturn, IEnumerable<object>> mapEvents)
    {
        CommandHandlerMappings.AddMapping<TCommand, THandler, TReturn>(mapEvents);
        return this;
    }

    /// <summary>
    /// Map an external command handler returning events and value
    /// </summary>
    /// <param name="mapEvents"></param>
    /// <param name="mapValue"></param>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    /// <typeparam name="TReturn"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <returns></returns>
    public Configuration MapCommandHandler<TCommand, THandler, TReturn, TValue>(
        Func<TReturn, IEnumerable<object>> mapEvents,
        Func<TReturn, TValue>? mapValue) where TValue : notnull
    {
        CommandHandlerMappings.AddMapping<TCommand, THandler, TReturn, TValue>(mapEvents, mapValue);
        return this;
    }

    /// <summary>
    /// Map an external command handler returning <see cref="IEnumerable{T}"/>
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    /// <typeparam name="THandler"></typeparam>
    /// <returns></returns>
    public Configuration MapCommandHandler<TCommand, THandler>()
    {
        CommandHandlerMappings.AddMapping<TCommand, THandler, IEnumerable<object>>(x => x);
        return this;
    }
}