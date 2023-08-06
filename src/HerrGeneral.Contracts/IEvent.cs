using System.Text;

namespace HerrGeneral.Contracts;

/// <summary>
/// Event interface (Write side)
/// </summary>
public interface IEvent
{
    /// <summary>
    /// Date of the event
    /// </summary>
    DateTime DateTimeEventOccurred { get; }
    
    /// <summary>
    /// Id of the event
    /// </summary>
    Guid EventId { get; }
    
    /// <summary>
    /// Id of the command at the origin of the event
    /// </summary>
    Guid SourceCommandId { get; }
}

