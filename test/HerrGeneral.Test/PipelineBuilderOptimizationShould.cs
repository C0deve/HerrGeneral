using HerrGeneral.WriteSide.Pipeline;

namespace HerrGeneral.Test.Pipeline;

public class PipelineBuilderOptimizationShould(ITestOutputHelper output)
{
    public record SimpleCommand(string Value);
    public record SimpleEvent(string Value);

    public class SimpleCommandHandler : ICommandHandler<SimpleCommand, string>
    {
        public (IReadOnlyList<object> Events, string Result) Handle(SimpleCommand command) =>
            ([new SimpleEvent(command.Value)], $"Processed:{command.Value}");
    }

    public class StepTracker
    {
        public List<string> Steps { get; } = [];
    }

    public class DiagnosticsBehavior<TCommand, TResult>(StepTracker tracker)
        : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
    {
        public PipelinePhase Phase => PipelinePhase.Diagnostics;
        public int OrderWithinPhase => 10;

        public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
            CommandExecutionContext<TCommand, TResult> context,
            CommandHandlerDelegate<TResult> nextHandler)
        {
            tracker.Steps.Add("DiagnosticsBehavior:Enter");
            var result = await nextHandler().ConfigureAwait(false);
            tracker.Steps.Add("DiagnosticsBehavior:Exit");
            return result;
        }
    }

    public class ResilienceBehavior<TCommand, TResult>(StepTracker tracker)
        : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
    {
        public PipelinePhase Phase => PipelinePhase.Resilience;
        public int OrderWithinPhase => 0;

        public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
            CommandExecutionContext<TCommand, TResult> context,
            CommandHandlerDelegate<TResult> nextHandler)
        {
            tracker.Steps.Add("ResilienceBehavior:Enter");
            var result = await nextHandler().ConfigureAwait(false);
            tracker.Steps.Add("ResilienceBehavior:Exit");
            return result;
        }
    }

    [Fact]
    public async Task Fast_path_without_custom_behaviors_executes_successfully()
    {
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton<SimpleCommandHandler>()
            .AddHerrGeneral(cfg => cfg.ScanWriteSideOn(typeof(SimpleCommandHandler).Assembly, typeof(SimpleCommandHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var result1 = await mediator.Send<string>(new SimpleCommand("First")).ShouldSuccess();
        result1.ShouldBe("Processed:First");

        var result2 = await mediator.Send<string>(new SimpleCommand("Second")).ShouldSuccess();
        result2.ShouldBe("Processed:Second");
    }

    [Fact]
    public async Task Custom_behaviors_across_multiple_phases_execute_in_correct_order()
    {
        var tracker = new StepTracker();
        var services = new ServiceCollection()
            .AddHerrGeneralTestLogger(output)
            .AddSingleton(tracker)
            .AddSingleton<SimpleCommandHandler>()
            .AddTransient(typeof(ICommandPipelineBehavior<,>), typeof(DiagnosticsBehavior<,>))
            .AddTransient(typeof(ICommandPipelineBehavior<,>), typeof(ResilienceBehavior<,>))
            .AddHerrGeneral(cfg => cfg.ScanWriteSideOn(typeof(SimpleCommandHandler).Assembly, typeof(SimpleCommandHandler).Namespace!));

        var sp = services.BuildServiceProvider();
        var mediator = sp.GetRequiredService<Mediator>();

        var result = await mediator.Send<string>(new SimpleCommand("Test")).ShouldSuccess();
        result.ShouldBe("Processed:Test");

        tracker.Steps.ShouldBe([
            "DiagnosticsBehavior:Enter",
            "ResilienceBehavior:Enter",
            "ResilienceBehavior:Exit",
            "DiagnosticsBehavior:Exit"
        ]);
    }
}
