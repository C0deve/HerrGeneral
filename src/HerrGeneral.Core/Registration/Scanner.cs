using System.Collections.ObjectModel;
using System.Reflection;
using HerrGeneral.Core.WriteSide;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Assembly scanner for HerrGeneral.Core types (ICommandHandler, write side IEventHandler, read side IEventHandler) 
/// </summary>
public class Scanner
{
    private readonly Type _commandHandlerInterface = typeof(ICommandHandler<,>);
    private readonly Type _writeSideEventHandlerInterface = typeof(IEventHandler<>);
    private readonly Type _readSideEventHandlerInterface = typeof(Contracts.ReadSIde.IEventHandler<>);
    private readonly HashSet<ScanParam> _writeSideSearchParams = new();
    private readonly HashSet<ScanParam> _readSideSearchParams = new();

    internal IReadOnlyCollection<Type> CommandHandlerTypes => _writeSideResult[_commandHandlerInterface];
    internal IReadOnlyCollection<Type> EventHandlerTypes => _writeSideResult[_writeSideEventHandlerInterface];
    internal IReadOnlyCollection<Type> ReadSideEventHandlerTypes => _readSideResult[_readSideEventHandlerInterface];

    private ReadOnlyDictionary<Type, HashSet<Type>> _writeSideResult = new(new Dictionary<Type, HashSet<Type>>());
    private ReadOnlyDictionary<Type, HashSet<Type>> _readSideResult = new(new Dictionary<Type, HashSet<Type>>());

    /// <summary>
    /// Add an assembly to scan for read side
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="namespaces"></param>
    /// <returns></returns>
    public Scanner OnReadSide(Assembly assembly, params string[] namespaces)
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
    public Scanner OnWriteSide(Assembly assembly, params string[] namespaces)
    {
        _writeSideSearchParams.Add(new ScanParam(assembly, namespaces));
        return this;
    }

    /// <summary>
    /// Scan on the provided ScanParams for write side and read side  
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Scanner Scan()
    {
        if (_writeSideSearchParams.Count == 0 && _readSideSearchParams.Count == 0) throw new InvalidOperationException("No assembly. Use On() to specify on which assemblies to scan");

        _writeSideResult = Scan(
            _writeSideSearchParams,
            _commandHandlerInterface,
            _writeSideEventHandlerInterface);

        _readSideResult = Scan(
            _readSideSearchParams,
            _readSideEventHandlerInterface);

        return this;
    }

    private static ReadOnlyDictionary<Type, HashSet<Type>> Scan(IEnumerable<ScanParam> scanParams, params Type[] openTypes) =>
        scanParams
            .SelectMany(scanParam => ScanForOpenTypes(scanParam, openTypes))
            .Aggregate(
                openTypes.ToDictionary(openType => openType, _ => new HashSet<Type>()),
                (dictionary, pair) =>
                {
                    dictionary[pair.Key].AddRange(pair.Value);
                    return dictionary;
                })
            .AsReadonly();
    
    private static ReadOnlyDictionary<Type, HashSet<Type>> ScanForOpenTypes(ScanParam scanParam, params Type[] openTypes) =>
        scanParam.Assembly.ScanForOpenTypes(
            scanParam.Namespaces,
            new HashSet<Type>(openTypes));
}