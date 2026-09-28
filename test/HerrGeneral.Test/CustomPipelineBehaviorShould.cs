using HerrGeneral.WriteSide.Pipeline;

namespace HerrGeneral.Test.Pipeline;

public class CustomPipelineBehaviorShould(ITestOutputHelper output)
{
    public record CreateUserCommand(string Username, int Age);

    public record UserCreatedEvent(string Username);

    public class ExecutionTracker
    {
        public List<string> Steps { get; } = [];
    }

    public class CreateUserHandler(ExecutionTracker tracker) : ICommandHandler<CreateUserCommand, string>
    {
        public (IReadOnlyList<object> Events, string Result) Handle(CreateUserCommand command)
        {
            tracker.Steps.Add("Handler:Execute");
            return ([new UserCreatedEvent(command.Username)], $"User-{command.Username}");
        }
    }

    public class ValidationBehavior<TCommand, TResult>(ExecutionTracker tracker)
        : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
    {
        public PipelinePhase Phase => PipelinePhase.Validation;
        public int OrderWithinPhase => 0;

        public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
            CommandExecutionContext<TCommand, TResult> context,
            CommandHandlerDelegate<TResult> nextHandler)
        {
            tracker.Steps.Add("Validation:Start");
            if (context.Command is CreateUserCommand { Age: < 18 })
            {
                throw new InvalidOperationException("User must be at least 18 years old");
            }

            var result = await nextHandler().ConfigureAwait(false);
            tracker.Steps.Add("Validation:End");
            return result;
        }
    }

    public class SecurityAuditBehavior<TCommand, TResult>(ExecutionTracker tracker)
        : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
    {
        public PipelinePhase Phase => PipelinePhase.Security;
        public int OrderWithinPhase => 0;

        public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
            CommandExecutionContext<TCommand, TResult> context,
            CommandHandlerDelegate<TResult> nextHandler)
        {
            tracker.Steps.Add("Security:Authorized");
            context.Items["AuditUser"] = "admin";
            var result = await nextHandler().ConfigureAwait(false);
            tracker.Steps.Add("Security:AuditLogged");
            return result;
        }
    }

    public class FastOrderBehavior<TCommand, TResult>(ExecutionTracker tracker)
        : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
    {
        public PipelinePhase Phase => PipelinePhase.Validation;
        public int OrderWithinPhase => -100;

        public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
            CommandExecutionContext<TCommand, TResult> context,
            CommandHandlerDelegate<TResult> nextHandler)
        {
            tracker.Steps.Add("Validation:FastCheck");
            return await nextHandler().ConfigureAwait(false);
        }
    }

    public class FakeUnitOfWork(ExecutionTracker tracker) : IUnitOfWork
    {
        public void Start() => tracker.Steps.Add("UnitOfWork:Start");
        public void Commit() => tracker.Steps.Add("UnitOfWork:Commit");
        public void RollBack() => tracker.Steps.Add("UnitOfWork:RollBack");
        public void Dispose() => tracker.Steps.Add("UnitOfWork:Dispose");
    }

    [Fact]
    public async Task Execute_custom_behaviors_in_correct_phase_order()
    {
        var tracker = new ExecutionTracker();
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(tracker)
            .AddSingleton<IUnitOfWork, FakeUnitOfWork>()
            .AddSingleton<CreateUserHandler>()
            .AddTransient(typeof(ICommandPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddTransient(typeof(ICommandPipelineBehavior<,>), typeof(SecurityAuditBehavior<,>))
            .AddTransient(typeof(ICommandPipelineBehavior<,>), typeof(FastOrderBehavior<,>))
            .AddHerrGeneral(cfg => cfg.ScanWriteSideOn(typeof(CreateUserHandler).Assembly, typeof(CreateUserHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var value = await mediator.Send<string>(new CreateUserCommand("Alice", 25)).ShouldSuccess();
        value.ShouldBe("User-Alice");

        tracker.Steps.ShouldBe([
            "Security:Authorized",
            "Validation:FastCheck",
            "Validation:Start",
            "UnitOfWork:Start",
            "Handler:Execute",
            "UnitOfWork:Commit",
            "UnitOfWork:Dispose",
            "Validation:End",
            "Security:AuditLogged"
        ]);
    }

    [Fact]
    public async Task Short_circuit_and_avoid_unit_of_work_when_validation_fails()
    {
        var tracker = new ExecutionTracker();
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(tracker)
            .AddSingleton<IUnitOfWork, FakeUnitOfWork>()
            .AddSingleton<CreateUserHandler>()
            .AddTransient(typeof(ICommandPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddHerrGeneral(cfg => cfg.ScanWriteSideOn(typeof(CreateUserHandler).Assembly, typeof(CreateUserHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        await mediator.Send<string>(new CreateUserCommand("Bob", 16)).ShouldFailWithPanicExceptionOfType<string, InvalidOperationException>();

        tracker.Steps.ShouldBe([
            "Validation:Start"
        ]);
    }

    public interface IAuditSink
    {
        void RecordAudit(string userId, string action, string commandPayload);
    }

    public class InMemoryAuditSink : IAuditSink
    {
        public List<(string UserId, string Action, string CommandPayload)> Audits { get; } = [];

        public void RecordAudit(string userId, string action, string commandPayload) =>
            Audits.Add((userId, action, commandPayload));
    }

    public class ContextPopulatingBehavior<TCommand, TResult>
        : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
    {
        public PipelinePhase Phase => PipelinePhase.Security;
        public int OrderWithinPhase => 10;

        public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
            CommandExecutionContext<TCommand, TResult> context,
            CommandHandlerDelegate<TResult> nextHandler)
        {
            context.Items["AuthenticatedUserId"] = "user-42";
            context.Items["TenantId"] = "tenant-xyz";

            return await nextHandler().ConfigureAwait(false);
        }
    }

    public class ContextConsumingBehavior<TCommand, TResult>(ExecutionTracker tracker)
        : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
    {
        public PipelinePhase Phase => PipelinePhase.Validation;
        public int OrderWithinPhase => 10;

        public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
            CommandExecutionContext<TCommand, TResult> context,
            CommandHandlerDelegate<TResult> nextHandler)
        {
            var userId = context.Items["AuthenticatedUserId"] as string;
            var tenantId = context.Items["TenantId"] as string;
            var isCancelled = context.CancellationToken.IsCancellationRequested;

            var auditSink = context.ServiceProvider.GetRequiredService<IAuditSink>();
            auditSink.RecordAudit(userId!, $"Tenant:{tenantId},Cancelled:{isCancelled}", context.Command?.ToString() ?? string.Empty);

            tracker.Steps.Add($"AuditRecorded:{userId}:{tenantId}");

            return await nextHandler().ConfigureAwait(false);
        }
    }

    [Fact]
    public async Task Transfer_context_items_resolve_services_and_access_cancellation_token_across_pipeline_behaviors()
    {
        var tracker = new ExecutionTracker();
        var auditSink = new InMemoryAuditSink();

        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(tracker)
            .AddSingleton<IAuditSink>(auditSink)
            .AddSingleton<IUnitOfWork, FakeUnitOfWork>()
            .AddSingleton<CreateUserHandler>()
            .AddTransient(typeof(ICommandPipelineBehavior<,>), typeof(ContextPopulatingBehavior<,>))
            .AddTransient(typeof(ICommandPipelineBehavior<,>), typeof(ContextConsumingBehavior<,>))
            .AddHerrGeneral(cfg => cfg.ScanWriteSideOn(typeof(CreateUserHandler).Assembly, typeof(CreateUserHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        using var cts = new CancellationTokenSource();
        var result = await mediator.Send<string>(new CreateUserCommand("David", 28), cts.Token).ShouldSuccess();

        result.ShouldBe("User-David");
        tracker.Steps.ShouldContain("AuditRecorded:user-42:tenant-xyz");

        auditSink.Audits.Count.ShouldBe(1);
        auditSink.Audits[0].UserId.ShouldBe("user-42");
        auditSink.Audits[0].Action.ShouldBe("Tenant:tenant-xyz,Cancelled:False");
        auditSink.Audits[0].CommandPayload.ShouldContain("David");
    }

    [Fact]
    public async Task Propagate_cancellation_token_cleanly()
    {
        var tracker = new ExecutionTracker();
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(tracker)
            .AddSingleton<CreateUserHandler>()
            .AddHerrGeneral(cfg => cfg.ScanWriteSideOn(typeof(CreateUserHandler).Assembly, typeof(CreateUserHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(async () =>
        {
            await mediator.Send<string>(new CreateUserCommand("Charlie", 30), cts.Token);
        });

        tracker.Steps.ShouldBeEmpty();
    }
}
