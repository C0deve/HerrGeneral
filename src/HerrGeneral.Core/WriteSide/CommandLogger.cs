using System.Collections.Concurrent;
using System.Text;

namespace HerrGeneral.Core.WriteSide;

internal class CommandLogger
{
    private readonly ConcurrentDictionary<Guid, StringBuilder> _stringBuilders = new();
    
    public StringBuilder GetStringBuilder(Guid commandId) =>
        commandId.WithValue(guid => _stringBuilders.GetOrAdd(guid, new StringBuilder()));

    public void RemoveStringBuilder(Guid commandId) => 
        commandId.WithValue(guid => _stringBuilders.TryRemove(guid, out _ ));
}