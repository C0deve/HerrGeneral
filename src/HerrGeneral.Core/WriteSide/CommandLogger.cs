using System.Collections.Concurrent;
using System.Text;

namespace HerrGeneral.Core.WriteSide;

internal class CommandLogger
{
    private readonly ConcurrentDictionary<UnitOfWorkId, StringBuilder> _stringBuilders = new();
    
    public StringBuilder GetStringBuilder(UnitOfWorkId unitOfWorkId) =>
        _stringBuilders.GetOrAdd(unitOfWorkId, new StringBuilder());

    public void RemoveStringBuilder(UnitOfWorkId unitOfWorkId) => 
        _stringBuilders.TryRemove(unitOfWorkId, out _ );
}