using System.Collections.ObjectModel;
using System.Reflection;
using HerrGeneral.WriteSide;

namespace HerrGeneral.Registration;

public record SearchParams(Assembly Assembly, HashSet<string> Namespaces)
{
    public SearchParams(Assembly Assembly, params string[] Namespaces) : this(Assembly, new HashSet<string>(Namespaces))
    {
    }

    public ReadOnlyDictionary<Type, HashSet<Type>> ScanForOpenTypes(params Type[] openTypes) =>
        Assembly.ScanForOpenTypes(
            Namespaces,
            new HashSet<Type>(openTypes));
}

public class Scanner
{
    private readonly Type _commandHandlerInterface = typeof(ICommandHandler<,>);
    private readonly Type _writeSideEventHandlerInterface = typeof(IEventHandler<>);
    private readonly Type _readSideEventHandlerInterface = typeof(ReadSide.Contracts.IEventHandler<>);
    private readonly HashSet<SearchParams> _writeSideSearchParams = new();
    private readonly HashSet<SearchParams> _readSideSearchParams = new();

    public IReadOnlyCollection<Type> CommandHandlerTypes => _writeSideResult[_commandHandlerInterface];
    public IReadOnlyCollection<Type> EventHandlerTypes => _writeSideResult[_writeSideEventHandlerInterface];
    public IReadOnlyCollection<Type> ReadSideEventHandlerTypes => _readSideResult[_readSideEventHandlerInterface];

    private ReadOnlyDictionary<Type, HashSet<Type>> _writeSideResult = new(new Dictionary<Type, HashSet<Type>>());
    private ReadOnlyDictionary<Type, HashSet<Type>> _readSideResult = new(new Dictionary<Type, HashSet<Type>>());

    public Scanner OnReadSide(Assembly assembly, params string[] namespaces)
    {
        _readSideSearchParams.Add(new SearchParams(assembly, namespaces));
        return this;
    }

    public Scanner OnWriteSide(Assembly assembly, params string[] namespaces)
    {
        _writeSideSearchParams.Add(new SearchParams(assembly, namespaces));
        return this;
    }

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

    private static ReadOnlyDictionary<Type, HashSet<Type>> Scan(IEnumerable<SearchParams> searchParams, params Type[] openTypes) =>
        searchParams
            .SelectMany(searchParam => searchParam.ScanForOpenTypes(openTypes))
            .Aggregate(
                openTypes.ToDictionary(openType => openType, _ => new HashSet<Type>()),
                (dictionary, pair) =>
                {
                    dictionary[pair.Key].AddRange(pair.Value);
                    return dictionary;
                })
            .AsReadonly();
}