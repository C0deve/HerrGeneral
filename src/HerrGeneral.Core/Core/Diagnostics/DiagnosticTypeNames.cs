using System.Collections.Concurrent;

namespace HerrGeneral.Core.Diagnostics;

/// <summary>
/// Names given to commands, events and handlers in diagnostics: the command name tag of activities and metrics,
/// and the causal tree written to the logs.
/// </summary>
/// <remarks>
/// By default a type is named by its short name, prefixed by its declaring types when it is nested. Types with the
/// same name in different namespaces cannot be told apart that way; only the application knows which part of the
/// namespace matters, so it can provide its own naming (see <c>ConfigurationBuilder.UseDiagnosticTypeNames</c>).
/// Event and handler type tags keep the full type name, which is already unambiguous.
/// </remarks>
internal sealed class DiagnosticTypeNames(Func<Type, string, string>? naming)
{
    public static readonly DiagnosticTypeNames Default = new(null);

    private readonly ConcurrentDictionary<Type, string> _names = new();

    public string Of(Type type) =>
        naming is null
            ? type.GetFriendlyName()
            : _names.GetOrAdd(type, Name);

    private string Name(Type type)
    {
        var defaultName = type.GetFriendlyName();
        var name = naming!(type, defaultName);
        return string.IsNullOrWhiteSpace(name) ? defaultName : name;
    }
}
