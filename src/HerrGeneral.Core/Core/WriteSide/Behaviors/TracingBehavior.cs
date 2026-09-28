using System.Diagnostics;
using HerrGeneral.Core.Diagnostics;
using HerrGeneral.WriteSide.Pipeline;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HerrGeneral.Core.WriteSide.Behaviors;

internal sealed class TracingBehavior<TCommand, TResult>(
    Type handlerType,
    ILogger<ICommandHandler<TCommand, TResult>>? logger,
    ActivityTreeCollector? activityCollector)
    : ICommandPipelineBehavior<TCommand, TResult>, IOrderedPipelineBehavior
{
    public PipelinePhase Phase => PipelinePhase.Diagnostics;
    public int OrderWithinPhase => 0;

    public async Task<(IReadOnlyList<object> Events, TResult Result)> HandleAsync(
        CommandExecutionContext<TCommand, TResult> context,
        CommandHandlerDelegate<TResult> next)
    {
        var log = logger ?? NullLogger<ICommandHandler<TCommand, TResult>>.Instance;

        var commandName = typeof(TCommand).GetFriendlyName();
        var threadId = Environment.CurrentManagedThreadId;

        using var activity = HerrGeneralDiagnostics.StartActivity(
            HerrGeneralDiagnostics.Activities.ExecuteCommand);

        activity?.SetTag(HerrGeneralDiagnostics.Tags.CommandName, commandName);
        activity?.SetTag(HerrGeneralDiagnostics.Tags.CommandType, typeof(TCommand).ToString());
        activity?.SetTag(HerrGeneralDiagnostics.Tags.HandlerType, handlerType.ToString());
        activity?.SetTag(HerrGeneralDiagnostics.Tags.ThreadId, threadId);

        HerrGeneralDiagnostics.ActiveCommands.Add(1, new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.CommandName, commandName));

        var watch = Stopwatch.StartNew();
        var status = "Success";

        activityCollector?.StartHandlingCommand(commandName, handlerType, threadId);

        try
        {
            var result = await next().ConfigureAwait(false);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (EventHandlerDomainException e)
        {
            status = "Error";
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
            activity?.RecordException(e);
            activityCollector?.RecordCommandException(e);
            throw;
        }
        catch (DomainException e)
        {
            status = "Error";
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
            activity?.RecordException(e);
            activityCollector?.RecordCommandException(e);
            throw;
        }
        catch (EventHandlerException e)
        {
            status = "Error";
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
            activity?.RecordException(e);
            activityCollector?.RecordCommandException(e);
            throw;
        }
        catch (System.Exception e)
        {
            status = "Error";
            activity?.SetStatus(ActivityStatusCode.Error, e.Message);
            activity?.RecordException(e);
            activityCollector?.RecordCommandException(e);
            throw;
        }
        finally
        {
            watch.Stop();
            HerrGeneralDiagnostics.ActiveCommands.Add(-1, new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.CommandName, commandName));

            HerrGeneralDiagnostics.CommandsTotal.Add(1,
                new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.CommandName, commandName),
                new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.Status, status));

            HerrGeneralDiagnostics.CommandsDuration.Record(watch.Elapsed.TotalMilliseconds,
                new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.CommandName, commandName),
                new KeyValuePair<string, object?>(HerrGeneralDiagnostics.Tags.Status, status));

            activityCollector?.StopHandlingCommand(commandName, watch.Elapsed);

            if (activityCollector is not null)
            {
                log.LogInformation("{Message}", ActivityTreeFormatter.Format(activityCollector));
            }
        }
    }
}
