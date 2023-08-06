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
    
    /// <summary>
    /// Used to control log format of the event
    /// </summary>
    /// <param name="sb"></param>
    /// <param name="indent"></param>
    /// <returns></returns>
    StringBuilder Log(StringBuilder sb, string indent);
}

