namespace HerrGeneral.WriteSide;

/// <summary>
/// Interface for command result
/// </summary>
public interface IWithSuccess
{
    /// <summary>
    /// The operation was successful
    /// </summary>
    bool IsSuccess { get; }
}