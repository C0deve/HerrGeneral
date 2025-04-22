using System.Collections.ObjectModel;
using System.Reflection;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Assembly scanner for HerrGeneral.Core types (ICommandHandler, write side IEventHandler, read side IEventHandler) 
/// </summary>
public class Scanner
{
    /// <summary>
    /// 
    /// </summary>
    public static readonly Type CommandHandlerInterfaceReturnType = typeof(ICommandHandler<,>);
    /// <summary>
    /// 
    /// </summary>
    public static readonly Type WriteSideEventHandlerInterface = typeof(IEventHandler<>);
    /// <summary>
    /// 
    /// </summary>
    public static readonly Type ReadSideEventHandlerInterface = typeof(HerrGeneral.ReadSide.IEventHandler<>);
    private readonly HashSet<ScanParam> _writeSideSearchParams = [];
    private readonly HashSet<ScanParam> _readSideSearchParams = [];

    internal IReadOnlyCollection<Type> CommandHandlerWithReturnTypes => _writeSideResult[CommandHandlerInterfaceReturnType];
    internal IReadOnlyCollection<Type> EventHandlerTypes => _writeSideResult[WriteSideEventHandlerInterface];
    internal IReadOnlyCollection<Type> ReadSideEventHandlerTypes => _readSideResult[ReadSideEventHandlerInterface];

    private ReadOnlyDictionary<Type, HashSet<Type>> _writeSideResult = new(new Dictionary<Type, HashSet<Type>>());
    private ReadOnlyDictionary<Type, HashSet<Type>> _readSideResult = new(new Dictionary<Type, HashSet<Type>>());

    /// <summary>
    /// Add an assembly to scan for read side
    /// </summary>
    /// <param name="assembly"></param>
    /// <param name="namespaces"></param>
    /// <returns></returns>
    public Scanner AddReadSideAssembly(Assembly assembly, params string[] namespaces)
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
    public Scanner AddWriteSideAssembly(Assembly assembly, params string[] namespaces)
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
            CommandHandlerInterfaceReturnType,
            WriteSideEventHandlerInterface);

        _readSideResult = Scan(
            _readSideSearchParams,
            ReadSideEventHandlerInterface);

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
            [..openTypes]);
}