namespace HerrGeneral.Test.PostProjections.Success
{
    public class HandlePostProjectionSuccessTests(ITestOutputHelper output)
    {
        public record RegisterUserCommand(Guid UserId, string Email);

        public record UserRegisteredEvent(Guid UserId, string Email);

        public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(RegisterUserCommand command) =>
                ([new UserRegisteredEvent(command.UserId, command.Email)], Unit.Default);
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

        public class UserReadModelProjection(ExecutionLog log) : IHandlePostProjection<UserRegisteredEvent>
        {
            public void Handle(UserRegisteredEvent @event) =>
                log.Entries.Add($"PostProjection:UpdateReadModel:{@event.Email}");
        }

        public class LegacyUserProjectionHandler(ExecutionLog log) : ReadSide.IProjectionEventHandler<UserRegisteredEvent>
        {
            public void Handle(UserRegisteredEvent @event) =>
                log.Entries.Add($"LegacyProjectionHandler:{@event.Email}");
        }

        [Fact]
        public async Task Execute_post_projection_handlers_after_unit_of_work_commit()
        {
            var log = new ExecutionLog();
            var uow = new FakeUnitOfWork(log);

            var services = new ServiceCollection()
                .AddHerrGeneralTestLogger(output)
                .AddSingleton(log)
                .AddSingleton<IUnitOfWork>(uow)
                .AddSingleton<UserReadModelProjection>()
                .AddSingleton<LegacyUserProjectionHandler>()
                .AddHerrGeneral(cfg => cfg
                    .ScanWriteSideOn(typeof(RegisterUserCommandHandler).Assembly, typeof(RegisterUserCommandHandler).Namespace!)
                    .ScanPostProjectionsOn(typeof(UserReadModelProjection).Assembly, typeof(UserReadModelProjection).Namespace!));

            var sp = services.BuildServiceProvider();
            var mediator = sp.GetRequiredService<Mediator>();

            var result = await mediator.Send(new RegisterUserCommand(Guid.NewGuid(), "alice@example.com"));

            result.IsSuccess.ShouldBeTrue();
            uow.IsCommitted.ShouldBeTrue();
            uow.IsRolledBack.ShouldBeFalse();

            var commitIndex = log.Entries.IndexOf("UoW:Commit");
            var postProjIndex = log.Entries.IndexOf("PostProjection:UpdateReadModel:alice@example.com");
            var legacyIndex = log.Entries.IndexOf("LegacyProjectionHandler:alice@example.com");

            commitIndex.ShouldBeGreaterThanOrEqualTo(0);
            postProjIndex.ShouldBeGreaterThan(commitIndex);
            legacyIndex.ShouldBeGreaterThan(commitIndex);
        }
    }
}

namespace HerrGeneral.Test.PostProjections.Failure
{
    public class HandlePostProjectionFailureTests(ITestOutputHelper output)
    {
        public record RegisterUserCommand(Guid UserId, string Email);

        public record UserRegisteredEvent(Guid UserId, string Email);

        public class RegisterUserCommandHandler : ICommandHandler<RegisterUserCommand, Unit>
        {
            public (IReadOnlyList<object> Events, Unit Result) Handle(RegisterUserCommand command) =>
                ([new UserRegisteredEvent(command.UserId, command.Email)], Unit.Default);
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

        public class FailingPostProjectionHandler(ExecutionLog log) : IHandlePostProjection<UserRegisteredEvent>
        {
            public void Handle(UserRegisteredEvent @event)
            {
                log.Entries.Add($"PostProjection:Failing:{@event.Email}");
                throw new InvalidOperationException("Read model store temporary outage");
            }
        }

        [Fact]
        public async Task Isolate_post_projection_failures_without_affecting_transaction_success()
        {
            var log = new ExecutionLog();
            var uow = new FakeUnitOfWork(log);

            var services = new ServiceCollection()
                .AddHerrGeneralTestLogger(output)
                .AddSingleton(log)
                .AddSingleton<IUnitOfWork>(uow)
                .AddSingleton<FailingPostProjectionHandler>()
                .AddHerrGeneral(cfg => cfg
                    .ScanWriteSideOn(typeof(RegisterUserCommandHandler).Assembly, typeof(RegisterUserCommandHandler).Namespace!)
                    .ScanPostProjectionsOn(typeof(FailingPostProjectionHandler).Assembly, typeof(FailingPostProjectionHandler).Namespace!));

            var sp = services.BuildServiceProvider();
            var mediator = sp.GetRequiredService<Mediator>();

            var result = await mediator.Send(new RegisterUserCommand(Guid.NewGuid(), "bob@example.com"));

            result.IsSuccess.ShouldBeTrue();
            uow.IsCommitted.ShouldBeTrue();
            uow.IsRolledBack.ShouldBeFalse();
            log.Entries.ShouldContain("PostProjection:Failing:bob@example.com");
        }
    }
}
