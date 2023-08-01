using System.Text;

namespace HerrGeneral.Contracts;

public interface IEvent
{
    DateTime DateTimeEventOccurred { get; }
    Guid EventId { get; }
    /// <summary>
    /// Id of the command at the origin of the event
    /// </summary>
    Guid SourceCommandId { get; }
    
    StringBuilder Log(StringBuilder sb, string indent);
}

