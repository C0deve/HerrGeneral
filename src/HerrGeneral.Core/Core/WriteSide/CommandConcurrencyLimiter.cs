namespace HerrGeneral.Core.WriteSide;

/// <summary>
/// Controls concurrency for mediator command execution using keyed (partitioned) locks and global limits.
/// </summary>
public sealed class CommandConcurrencyLimiter : IDisposable
{
    private static readonly ConcurrentDictionary<Type, Func<object, object?>?> KeyExtractors = new();

    private readonly SemaphoreSlim _globalSemaphore;
    private readonly Lock _syncRoot = new();
    private readonly Dictionary<object, KeyedLockEntry> _keyedLocks = new();
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of <see cref="CommandConcurrencyLimiter"/>.
    /// </summary>
    /// <param name="maxConcurrentCommands">The maximum number of concurrent unkeyed/global commands.</param>
    public CommandConcurrencyLimiter(int maxConcurrentCommands = 1) => 
        _globalSemaphore = new SemaphoreSlim(maxConcurrentCommands, maxConcurrentCommands);

    /// <summary>
    /// Acquires a lock for the specified command or key.
    /// </summary>
    /// <param name="command">The command or key object to lock on.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>An <see cref="IDisposable"/> that releases the lock when disposed.</returns>
    public async Task<IDisposable> AcquireAsync(object? command, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var key = ExtractKey(command);
        if (key is null)
        {
            await _globalSemaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new GlobalReleaser(_globalSemaphore);
        }

        KeyedLockEntry entry;
        lock (_syncRoot)
        {
            if (!_keyedLocks.TryGetValue(key, out entry!))
            {
                entry = new KeyedLockEntry();
                _keyedLocks.Add(key, entry);
            }
            else
            {
                entry.RefCount++;
            }
        }

        try
        {
            await entry.Semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
            return new KeyedReleaser(this, key, entry);
        }
        catch
        {
            lock (_syncRoot)
            {
                entry.RefCount--;
                if (entry.RefCount == 0)
                {
                    _keyedLocks.Remove(key);
                    entry.Semaphore.Dispose();
                }
            }
            throw;
        }
    }

    /// <summary>
    /// Waits asynchronously to enter the concurrency limiter for unkeyed commands.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the wait operation.</returns>
    public Task WaitAsync(CancellationToken cancellationToken) => _globalSemaphore.WaitAsync(cancellationToken);

    /// <summary>
    /// Releases the unkeyed concurrency limiter.
    /// </summary>
    public void Release() => _globalSemaphore.Release();

    /// <summary>
    /// Extracts the partition/lock key from a command.
    /// </summary>
    /// <param name="command">The command to inspect.</param>
    /// <returns>The extracted key, or null if unkeyed.</returns>
    public static object? ExtractKey(object? command)
    {
        switch (command)
        {
            case null:
                return null;
            case IKeyedCommand keyedCommand:
                return keyedCommand.Key;
            default:
            {
                var extractor = KeyExtractors.GetOrAdd(command.GetType(), CreateKeyExtractor);
                return extractor?.Invoke(command);
            }
        }
    }

    private static Func<object, object?>? CreateKeyExtractor(Type type)
    {
        if (typeof(IKeyedCommand).IsAssignableFrom(type))
        {
            return cmd => ((IKeyedCommand)cmd).Key;
        }

        var prop = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p => p.CanRead && p.GetCustomAttribute<LockKeyAttribute>(true) != null);

        if (prop is null)
            return null;

        var attr = prop.GetCustomAttribute<LockKeyAttribute>(true)!;
        var keyGroup = attr.KeyGroup;

        var param = Expression.Parameter(typeof(object), "cmd");
        var typedParam = Expression.Convert(param, type);
        var propAccess = Expression.Property(typedParam, prop);
        var convertToObject = Expression.Convert(propAccess, typeof(object));

        var keyGroupExpr = Expression.Constant(keyGroup, typeof(string));
        var tupleCtor = typeof(ValueTuple<string, object>).GetConstructor([typeof(string), typeof(object)])!;
        var tupleExpr = Expression.Convert(
            Expression.New(tupleCtor, keyGroupExpr, convertToObject),
            typeof(object));

        var nullCheck = Expression.Condition(
            Expression.Equal(convertToObject, Expression.Constant(null, typeof(object))),
            Expression.Constant(null, typeof(object)),
            tupleExpr);

        return Expression.Lambda<Func<object, object?>>(nullCheck, param).Compile();
    }

    private void ReleaseKeyed(object key, KeyedLockEntry entry)
    {
        lock (_syncRoot)
        {
            entry.Semaphore.Release();
            entry.RefCount--;
            if (entry.RefCount == 0)
            {
                _keyedLocks.Remove(key);
                entry.Semaphore.Dispose();
            }
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        lock (_syncRoot)
        {
            foreach (var entry in _keyedLocks.Values)
            {
                entry.Semaphore.Dispose();
            }
            _keyedLocks.Clear();
        }
        _globalSemaphore.Dispose();
    }

    private sealed class KeyedLockEntry
    {
        public readonly SemaphoreSlim Semaphore = new(1, 1);
        public int RefCount = 1;
    }

    private sealed class KeyedReleaser(CommandConcurrencyLimiter owner, object key, KeyedLockEntry entry) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                owner.ReleaseKeyed(key, entry);
            }
        }
    }

    private sealed class GlobalReleaser(SemaphoreSlim semaphore) : IDisposable
    {
        private int _disposed;

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                semaphore.Release();
            }
        }
    }
}
