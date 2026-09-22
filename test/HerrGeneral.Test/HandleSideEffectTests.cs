namespace HerrGeneral.Test.SideEffects.Success
{
    public class HandleSideEffectSuccessTests(ITestOutputHelper output)
    {
        public record CreateItemCommand(Guid Id, string Name);

        public record ItemCreatedEvent(Guid Id, string Name);

        public class CreateItemCommandHandler : ICommandHandler<CreateItemCommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(CreateItemCommand command) =>
                ([new ItemCreatedEvent(command.Id, command.Name)], Unit.Default);
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

        public class NotificationSideEffectHandler(ExecutionLog log) : IHandleSideEffect<ItemCreatedEvent>
        {
            public void Handle(ItemCreatedEvent @event) =>
                log.Entries.Add($"SideEffect:Notify:{@event.Name}");
        }

        [Fact]
        public async Task Execute_side_effects_after_unit_of_work_commit()
        {
            var log = new ExecutionLog();
            var uow = new FakeUnitOfWork(log);

            var services = new ServiceCollection()
                .AddHerrGeneralTestLogger(output)
                .AddSingleton(log)
                .AddSingleton<IUnitOfWork>(uow)
                .AddSingleton<NotificationSideEffectHandler>()
                .AddHerrGeneral(cfg => cfg
                    .ScanWriteSideOn(typeof(CreateItemCommandHandler).Assembly, typeof(CreateItemCommandHandler).Namespace!)
                    .ScanSideEffectsOn(typeof(NotificationSideEffectHandler).Assembly, typeof(NotificationSideEffectHandler).Namespace!));

            var sp = services.BuildServiceProvider();
            var mediator = sp.GetRequiredService<Mediator>();

            var result = await mediator.Send(new CreateItemCommand(Guid.NewGuid(), "SampleItem"));

            result.IsSuccess.ShouldBeTrue();
            uow.IsCommitted.ShouldBeTrue();
            uow.IsRolledBack.ShouldBeFalse();

            var commitIndex = log.Entries.IndexOf("UoW:Commit");
            var sideEffectIndex = log.Entries.IndexOf("SideEffect:Notify:SampleItem");

            commitIndex.ShouldBeGreaterThanOrEqualTo(0);
            sideEffectIndex.ShouldBeGreaterThan(commitIndex);
        }
    }
}

namespace HerrGeneral.Test.SideEffects.Failure
{
    public class HandleSideEffectFailureTests(ITestOutputHelper output)
    {
        public record CreateItemCommand(Guid Id, string Name);

        public record ItemCreatedEvent(Guid Id, string Name);

        public class CreateItemCommandHandler : ICommandHandler<CreateItemCommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(CreateItemCommand command) =>
                ([new ItemCreatedEvent(command.Id, command.Name)], Unit.Default);
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

        public class FailingSideEffectHandler(ExecutionLog log) : IHandleSideEffect<ItemCreatedEvent>
        {
            public void Handle(ItemCreatedEvent @event)
            {
                log.Entries.Add($"SideEffect:Failing:{@event.Name}");
                throw new InvalidOperationException("External service failure");
            }
        }

        [Fact]
        public async Task Isolate_side_effect_failures_without_rolling_back_committed_transaction()
        {
            var log = new ExecutionLog();
            var uow = new FakeUnitOfWork(log);

            var services = new ServiceCollection()
                .AddHerrGeneralTestLogger(output)
                .AddSingleton(log)
                .AddSingleton<IUnitOfWork>(uow)
                .AddSingleton<FailingSideEffectHandler>()
                .AddHerrGeneral(cfg => cfg
                    .ScanWriteSideOn(typeof(CreateItemCommandHandler).Assembly, typeof(CreateItemCommandHandler).Namespace!)
                    .ScanSideEffectsOn(typeof(FailingSideEffectHandler).Assembly, typeof(FailingSideEffectHandler).Namespace!));

            var sp = services.BuildServiceProvider();
            var mediator = sp.GetRequiredService<Mediator>();

            var result = await mediator.Send(new CreateItemCommand(Guid.NewGuid(), "FailingItem"));

            // Command result must reflect domain success despite side-effect failure
            result.IsSuccess.ShouldBeTrue();
            uow.IsCommitted.ShouldBeTrue();
            uow.IsRolledBack.ShouldBeFalse();
            log.Entries.ShouldContain("SideEffect:Failing:FailingItem");
        }
    }
}
