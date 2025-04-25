using System.Collections.ObjectModel;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Core.Registration;

/// <summary>
/// Assembly scanner for HerrGeneral.Core types (ICommandHandler, write side IEventHandler, read side IEventHandler) 
/// </summary>
public class Scanner
{
    /// <summary>
    /// Command handler interface
    /// </summary>
    public static readonly Type CommandHandlerInterfaceReturnType = typeof(ICommandHandler<,>);

    /// <summary>
    /// Event handler interface from write side 
    /// </summary>
    public static readonly Type WriteSideEventHandlerInterface = typeof(IEventHandler<>);

    /// <summary>
    /// Event handler interface from read side
    /// </summary>
    public static readonly Type ReadSideEventHandlerInterface = typeof(HerrGeneral.ReadSide.IEventHandler<>);


    internal IReadOnlyCollection<Type> CommandHandlerWithReturnTypes => _writeSideResult[CommandHandlerInterfaceReturnType];
    internal IReadOnlyCollection<Type> EventHandlerTypes => _writeSideResult[WriteSideEventHandlerInterface];
    internal IReadOnlyCollection<Type> ReadSideEventHandlerTypes => _readSideResult[ReadSideEventHandlerInterface];

    private ReadOnlyDictionary<Type, HashSet<Type>> _writeSideResult = new(new Dictionary<Type, HashSet<Type>>());
    private ReadOnlyDictionary<Type, HashSet<Type>> _readSideResult = new(new Dictionary<Type, HashSet<Type>>());

    private readonly Configuration _configuration;

    /// <summary>
    /// Ctor
    /// </summary>
    /// <param name="configuration"></param>
    public Scanner(Configuration configuration) => _configuration = configuration;

    /// <summary>
    /// Scan on the provided ScanParams for write side and read side  
    /// </summary>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public Scanner Scan()
    {
        _configuration.ThrowIfNotValid();

        _writeSideResult = Scan(_configuration.WriteSideSearchParams,
            CommandHandlerInterfaceReturnType,
            WriteSideEventHandlerInterface);

        _readSideResult = Scan(_configuration.ReadSideSearchParams,
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