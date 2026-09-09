using System.Collections.Concurrent;
using System.Diagnostics;
using HerrGeneral.Core.WriteSide;

// ReSharper disable once CheckNamespace
namespace HerrGeneral.Concurrency.Test;

public class KeyedConcurrencyLimiterShould(ITestOutputHelper output)
{
    private record KeyedCmd(string PartitionKey, int DelayMs) : IKeyedCommand
    {
        public object Key => PartitionKey;
    }

    private class KeyedHandler(ConcurrentBag<string> log, ConcurrentDictionary<string, int> activeCount)
        : ICommandHandler<KeyedCmd, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(KeyedCmd command)
        {
            var active = activeCount.AddOrUpdate(command.PartitionKey, 1, (_, current) => current + 1);
            if (active > 1)
            {
                log.Add($"CONCURRENCY_VIOLATION:{command.PartitionKey}:{active}");
            }

            log.Add($"START:{command.PartitionKey}");
            Thread.Sleep(command.DelayMs);
            log.Add($"END:{command.PartitionKey}");

            activeCount.AddOrUpdate(command.PartitionKey, 0, (_, current) => current - 1);
            return ([], Unit.Default);
        }
    }

    private record LockKeyCmd([property: LockKey<DummyOrder>] Guid AggregateId, int DelayMs);

    private class LockKeyHandler(ConcurrentBag<string> log, ConcurrentDictionary<Guid, int> activeCount)
        : ICommandHandler<LockKeyCmd, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(LockKeyCmd command)
        {
            var active = activeCount.AddOrUpdate(command.AggregateId, 1, (_, current) => current + 1);
            if (active > 1)
            {
                log.Add($"CONCURRENCY_VIOLATION:{command.AggregateId}:{active}");
            }

            log.Add($"START:{command.AggregateId}");
            Thread.Sleep(command.DelayMs);
            log.Add($"END:{command.AggregateId}");

            activeCount.AddOrUpdate(command.AggregateId, 0, (_, current) => current - 1);
            return ([], Unit.Default);
        }
    }

    private record DummyOrder;

    private record OrderCmd([property: LockKey<DummyOrder>] Guid Id, Barrier Barrier, ConcurrentBag<string> Log);
    private class OrderHandler : ICommandHandler<OrderCmd, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(OrderCmd command)
        {
            command.Log.Add($"ORDER_START:{command.Id}");
            command.Barrier.SignalAndWait(TimeSpan.FromSeconds(2));
            command.Log.Add($"ORDER_END:{command.Id}");
            return ([], Unit.Default);
        }
    }

    private record UserCmd([property: LockKey<DummyOrder>] Guid Id, Barrier Barrier, ConcurrentBag<string> Log);
    private class UserHandler : ICommandHandler<UserCmd, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(UserCmd command)
        {
            command.Log.Add($"USER_START:{command.Id}");
            command.Barrier.SignalAndWait(TimeSpan.FromSeconds(2));
            command.Log.Add($"USER_END:{command.Id}");
            return ([], Unit.Default);
        }
    }

    private record UnannotatedCmd(Guid AggregateId);

    private record NullableKeyCmd([property: LockKey<DummyOrder>] string? NullableKey);

    private record OrderWithDelayCmd([property: LockKey<DummyOrder>] Guid Id, int DelayMs);
    private class OrderWithDelayHandler(ConcurrentBag<string> log, ConcurrentDictionary<Guid, int> activeCount)
        : ICommandHandler<OrderWithDelayCmd, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(OrderWithDelayCmd command)
        {
            var active = activeCount.AddOrUpdate(command.Id, 1, (_, current) => current + 1);
            if (active > 1)
            {
                log.Add($"CONCURRENCY_VIOLATION:{command.Id}:{active}");
            }

            log.Add($"START:{command.Id}");
            Thread.Sleep(command.DelayMs);
            log.Add($"END:{command.Id}");

            activeCount.AddOrUpdate(command.Id, 0, (_, current) => current - 1);
            return ([], Unit.Default);
        }
    }

    private record BarrierCmd(string PartitionKey) : IKeyedCommand
    {
        public object Key => PartitionKey;
    }

    private class BarrierKeyedHandler(Barrier barrier, ConcurrentBag<string> log)
        : ICommandHandler<BarrierCmd, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(BarrierCmd command)
        {
            log.Add($"START:{command.PartitionKey}");
            var reached = barrier.SignalAndWait(TimeSpan.FromSeconds(2));
            if (!reached)
            {
                log.Add($"TIMEOUT:{command.PartitionKey}");
            }
            log.Add($"END:{command.PartitionKey}");
            return ([], Unit.Default);
        }
    }

    private record ControllableCmd(string PartitionKey, ManualResetEventSlim Started, ManualResetEventSlim Release) : IKeyedCommand
    {
        public object Key => PartitionKey;
    }

    private class ControllableHandler : ICommandHandler<ControllableCmd, Unit>
    {
        public (IReadOnlyList<object> Events, Unit Result) Handle(ControllableCmd command)
        {
            command.Started.Set();
            command.Release.Wait();
            return ([], Unit.Default);
        }
    }

    [Fact]
    public async Task Execute_commands_with_different_keys_concurrently()
    {
        var log = new ConcurrentBag<string>();
        using var barrier = new Barrier(2);

        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(log)
            .AddSingleton(barrier)
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(BarrierKeyedHandler).Assembly, typeof(BarrierKeyedHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var task1 = Task.Run(() => mediator.Send(new BarrierCmd("KeyA")));
        var task2 = Task.Run(() => mediator.Send(new BarrierCmd("KeyB")));

        await Task.WhenAll(task1, task2);

        log.ShouldNotContain(entry => entry.StartsWith("TIMEOUT"));
        log.Count(entry => entry.StartsWith("START:")).ShouldBe(2);
        log.Count(entry => entry.StartsWith("END:")).ShouldBe(2);
    }

    [Fact]
    public async Task Serialize_commands_with_same_key()
    {
        var log = new ConcurrentBag<string>();
        var activeCount = new ConcurrentDictionary<string, int>();

        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(log)
            .AddSingleton(activeCount)
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(KeyedHandler).Assembly, typeof(KeyedHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var sw = Stopwatch.StartNew();

        var task1 = Task.Run(() => mediator.Send(new KeyedCmd("SameKey", 100)));
        var task2 = Task.Run(() => mediator.Send(new KeyedCmd("SameKey", 100)));

        await Task.WhenAll(task1, task2);
        sw.Stop();

        // If executed sequentially, total time is at least 200ms
        sw.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(180);
        log.ShouldNotContain(entry => entry.StartsWith("CONCURRENCY_VIOLATION"));
    }

    [Fact]
    public async Task Serialize_commands_with_lock_key_attribute()
    {
        var log = new ConcurrentBag<string>();
        var activeCount = new ConcurrentDictionary<Guid, int>();

        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(log)
            .AddSingleton(activeCount)
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(LockKeyHandler).Assembly, typeof(LockKeyHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var aggregateId = Guid.NewGuid();
        var sw = Stopwatch.StartNew();

        var task1 = Task.Run(() => mediator.Send(new LockKeyCmd(aggregateId, 100)));
        var task2 = Task.Run(() => mediator.Send(new LockKeyCmd(aggregateId, 100)));

        await Task.WhenAll(task1, task2);
        sw.Stop();

        sw.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(180);
        log.ShouldNotContain(entry => entry.StartsWith("CONCURRENCY_VIOLATION"));
    }

    [Fact]
    public async Task Execute_concurrently_when_commands_have_same_id_but_different_aggregate_types()
    {
        var log = new ConcurrentBag<string>();
        using var barrier = new Barrier(2);

        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(log)
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(OrderHandler).Assembly, typeof(OrderHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var sameId = Guid.NewGuid();

        var task1 = Task.Run(() => mediator.Send(new OrderCmd(sameId, barrier, log)));
        var task2 = Task.Run(() => mediator.Send(new UserCmd(sameId, barrier, log)));

        await Task.WhenAll(task1, task2);

        log.Count(entry => entry.StartsWith("ORDER_START")).ShouldBe(1);
        log.Count(entry => entry.StartsWith("USER_START")).ShouldBe(1);
        log.Count(entry => entry.StartsWith("ORDER_END")).ShouldBe(1);
        log.Count(entry => entry.StartsWith("USER_END")).ShouldBe(1);
    }

    [Fact]
    public async Task Serialize_commands_with_same_aggregate_type_and_id()
    {
        var log = new ConcurrentBag<string>();
        var activeCount = new ConcurrentDictionary<Guid, int>();

        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(log)
            .AddSingleton(activeCount)
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(OrderWithDelayHandler).Assembly, typeof(OrderWithDelayHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var orderId = Guid.NewGuid();
        var sw = Stopwatch.StartNew();

        var task1 = Task.Run(() => mediator.Send(new OrderWithDelayCmd(orderId, 100)));
        var task2 = Task.Run(() => mediator.Send(new OrderWithDelayCmd(orderId, 100)));

        await Task.WhenAll(task1, task2);
        sw.Stop();

        sw.ElapsedMilliseconds.ShouldBeGreaterThanOrEqualTo(180);
        log.ShouldNotContain(entry => entry.StartsWith("CONCURRENCY_VIOLATION"));
    }

    [Fact]
    public void Return_null_when_no_lock_key_or_keyed_interface_is_present()
    {
        var cmd = new UnannotatedCmd(Guid.NewGuid());
        var key = CommandConcurrencyLimiter.ExtractKey(cmd);
        key.ShouldBeNull();
    }

    [Fact]
    public void Return_null_when_lock_key_property_is_null_or_command_is_null()
    {
        CommandConcurrencyLimiter.ExtractKey(null).ShouldBeNull();

        var nullKeyCmd = new NullableKeyCmd(null);
        CommandConcurrencyLimiter.ExtractKey(nullKeyCmd).ShouldBeNull();

        var nonNullKeyCmd = new NullableKeyCmd("abc");
        CommandConcurrencyLimiter.ExtractKey(nonNullKeyCmd).ShouldBe((typeof(DummyOrder).FullName, (object)"abc"));
    }

    [Fact]
    public void Extract_qualified_key_properly()
    {
        var id = Guid.NewGuid();
        var cmdWithAttr = new LockKeyCmd(id, 0);
        var key = CommandConcurrencyLimiter.ExtractKey(cmdWithAttr);
        key.ShouldBe((typeof(DummyOrder).FullName, (object)id));

        using var barrier = new Barrier(1);
        var log = new ConcurrentBag<string>();
        var orderCmd = new OrderCmd(id, barrier, log);
        var orderKey = CommandConcurrencyLimiter.ExtractKey(orderCmd);
        orderKey.ShouldBe((typeof(DummyOrder).FullName, (object)id));
    }

    [Fact]
    public async Task Cancel_waiting_keyed_command_without_breaking_subsequent_commands()
    {
        var log = new ConcurrentBag<string>();
        var activeCount = new ConcurrentDictionary<string, int>();

        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(log)
            .AddSingleton(activeCount)
            .AddHerrGeneral(scanner =>
                scanner.ScanWriteSideOn(typeof(KeyedHandler).Assembly, typeof(KeyedHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        using var started1 = new ManualResetEventSlim(false);
        using var release1 = new ManualResetEventSlim(false);

        // Start long running command on KeyX
        var task1 = Task.Run(() => mediator.Send(new ControllableCmd("KeyX", started1, release1)));

        // Wait until task1 has acquired the lock and is running
        started1.Wait(TimeSpan.FromSeconds(2)).ShouldBeTrue();

        // Start second command on same key with cancellation token
        using var cts = new CancellationTokenSource();
        var task2 = Task.Run(() => mediator.Send(new KeyedCmd("KeyX", 100), cts.Token));

        // Allow task2 to enter AcquireAsync and start waiting on the semaphore
        await Task.Delay(50);
        await cts.CancelAsync();

        await Should.ThrowAsync<OperationCanceledException>(async () => await task2);

        // Unblock first command and verify success
        release1.Set();
        var result1 = await task1;
        result1.IsSuccess.ShouldBeTrue();

        // Third command on KeyX should acquire lock normally and succeed
        var result3 = await mediator.Send(new KeyedCmd("KeyX", 50));
        result3.IsSuccess.ShouldBeTrue();
    }
}
