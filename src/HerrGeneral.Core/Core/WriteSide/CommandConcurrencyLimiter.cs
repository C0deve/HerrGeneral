namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// Controls concurrency for mediator command execution.
/// </summary>
public sealed class CommandConcurrencyLimiter(int maxConcurrentCommands)
{
    private readonly SemaphoreSlim _semaphoreSlim = new(maxConcurrentCommands, maxConcurrentCommands);

    /// <summary>
    /// Waits asynchronously to enter the concurrency limiter.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the wait operation.</returns>
    public Task WaitAsync(CancellationToken cancellationToken) => _semaphoreSlim.WaitAsync(cancellationToken);

    /// <summary>
    /// Releases the concurrency limiter.
    /// </summary>
    public void Release() => _semaphoreSlim.Release();
}
