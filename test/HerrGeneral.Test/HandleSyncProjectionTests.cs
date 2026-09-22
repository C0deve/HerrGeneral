namespace HerrGeneral.Test.SyncProjections.Success
{
    public class HandleSyncProjectionSuccessTests(ITestOutputHelper output)
    {
        public record AddMoneyCommand(Guid AccountId, decimal Amount);

        public record MoneyAddedEvent(Guid AccountId, decimal Amount);

        public class AddMoneyCommandHandler : ICommandHandler<AddMoneyCommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(AddMoneyCommand command) =>
                ([new MoneyAddedEvent(command.AccountId, command.Amount)], Unit.Default);
        }

        public class ExecutionLog
        {
            public List<string> Entries { get; } = [];
        }

        public class FakeUnitOfWork(ExecutionLog log) : IUnitOfWork
        {
            public bool IsCommitted { get; private set; }
            public bool IsRolledBack { get; private set; }

            public void Start() => log.Entries.Add("UoW:Start");
            public void Commit()
            {
                IsCommitted = true;
                log.Entries.Add("UoW:Commit");
            }
            public void RollBack()
            {
                IsRolledBack = true;
                log.Entries.Add("UoW:RollBack");
            }
            public void Dispose() => log.Entries.Add("UoW:Dispose");
        }

        public class SuccessfulSyncProjectionHandler(ExecutionLog log) : IHandleSyncProjection<MoneyAddedEvent>
        {
            public void Handle(MoneyAddedEvent @event) =>
                log.Entries.Add($"SyncProjection:Success:{@event.Amount}");
        }

        [Fact]
        public async Task Execute_sync_projection_inside_unit_of_work_before_commit()
        {
            var log = new ExecutionLog();
            var uow = new FakeUnitOfWork(log);

            var services = new ServiceCollection()
                .AddHerrGeneralTestLogger(output)
                .AddSingleton(log)
                .AddSingleton<IUnitOfWork>(uow)
                .AddSingleton<SuccessfulSyncProjectionHandler>()
                .AddHerrGeneral(cfg => cfg
                    .ScanWriteSideOn(typeof(AddMoneyCommandHandler).Assembly, typeof(AddMoneyCommandHandler).Namespace!)
                    .ScanSyncProjectionsOn(typeof(SuccessfulSyncProjectionHandler).Assembly, typeof(SuccessfulSyncProjectionHandler).Namespace!));

            var sp = services.BuildServiceProvider();
            var mediator = sp.GetRequiredService<Mediator>();

            var result = await mediator.Send(new AddMoneyCommand(Guid.NewGuid(), 100m));

            result.IsSuccess.ShouldBeTrue();
            uow.IsCommitted.ShouldBeTrue();
            uow.IsRolledBack.ShouldBeFalse();

            var syncIndex = log.Entries.IndexOf("SyncProjection:Success:100");
            var commitIndex = log.Entries.IndexOf("UoW:Commit");

            syncIndex.ShouldBeGreaterThanOrEqualTo(0);
            commitIndex.ShouldBeGreaterThan(syncIndex);
        }
    }
}

namespace HerrGeneral.Test.SyncProjections.Failure
{
    public class HandleSyncProjectionFailureTests(ITestOutputHelper output)
    {
        public record AddMoneyCommand(Guid AccountId, decimal Amount);

        public record MoneyAddedEvent(Guid AccountId, decimal Amount);

        public class AddMoneyCommandHandler : ICommandHandler<AddMoneyCommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(AddMoneyCommand command) =>
                ([new MoneyAddedEvent(command.AccountId, command.Amount)], Unit.Default);
        }

        public class ExecutionLog
        {
            public List<string> Entries { get; } = [];
        }

        public class FakeUnitOfWork(ExecutionLog log) : IUnitOfWork
        {
            public bool IsCommitted { get; private set; }
            public bool IsRolledBack { get; private set; }

            public void Start() => log.Entries.Add("UoW:Start");
            public void Commit()
            {
                IsCommitted = true;
                log.Entries.Add("UoW:Commit");
            }
            public void RollBack()
            {
                IsRolledBack = true;
                log.Entries.Add("UoW:RollBack");
            }
            public void Dispose() => log.Entries.Add("UoW:Dispose");
        }

        public class FailingSyncProjectionHandler(ExecutionLog log) : IHandleSyncProjection<MoneyAddedEvent>
        {
            public void Handle(MoneyAddedEvent @event)
            {
                log.Entries.Add($"SyncProjection:Failing:{@event.Amount}");
                throw new InvalidOperationException("Database constraint violation in projection");
            }
        }

        public class PostCommitProjectionHandler(ExecutionLog log) : IHandlePostProjection<MoneyAddedEvent>
        {
            public void Handle(MoneyAddedEvent @event) =>
                log.Entries.Add($"PostProjection:{@event.Amount}");
        }

        [Fact]
        public async Task Rollback_unit_of_work_and_suppress_post_transaction_handlers_when_sync_projection_fails()
        {
            var log = new ExecutionLog();
            var uow = new FakeUnitOfWork(log);

            var services = new ServiceCollection()
                .AddHerrGeneralTestLogger(output)
                .AddSingleton(log)
                .AddSingleton<IUnitOfWork>(uow)
                .AddSingleton<FailingSyncProjectionHandler>()
                .AddSingleton<PostCommitProjectionHandler>()
                .AddHerrGeneral(cfg => cfg
                    .ScanWriteSideOn(typeof(AddMoneyCommandHandler).Assembly, typeof(AddMoneyCommandHandler).Namespace!)
                    .ScanSyncProjectionsOn(typeof(FailingSyncProjectionHandler).Assembly, typeof(FailingSyncProjectionHandler).Namespace!)
                    .ScanPostProjectionsOn(typeof(PostCommitProjectionHandler).Assembly, typeof(PostCommitProjectionHandler).Namespace!));

            var sp = services.BuildServiceProvider();
            var mediator = sp.GetRequiredService<Mediator>();

            var result = await mediator.Send(new AddMoneyCommand(Guid.NewGuid(), 500m));

            // Result must indicate failure
            result.IsSuccess.ShouldBeFalse();
            uow.IsCommitted.ShouldBeFalse();
            uow.IsRolledBack.ShouldBeTrue();

            log.Entries.ShouldContain("SyncProjection:Failing:500");
            log.Entries.ShouldNotContain("PostProjection:500");
        }
    }
}
