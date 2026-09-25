using System.Diagnostics;
using HerrGeneral.Core.Diagnostics;

namespace HerrGeneral.Core.WriteSide.Tracer;

/// <summary>
/// Decorator for IUnitOfWork that adds operation tracing via ActivitySource and CommandExecutionTracer.
/// This decorator wraps an existing IUnitOfWork instance and traces all operations.
/// </summary>
internal class UnitOfWorkTraceDecorator(IUnitOfWork innerUnitOfWork, ActivityTreeCollector? collector = null)
    : IUnitOfWork
{
    private Activity? _activity;

    public void Start()
    {
        _activity = HerrGeneralDiagnostics.StartActivity(
            HerrGeneralDiagnostics.Activities.UnitOfWork);
        _activity?.AddEvent(new ActivityEvent("Start"));

        var watch = Stopwatch.StartNew();
        innerUnitOfWork.Start();
        watch.Stop();
        collector?.StartUnitOfWork(watch.Elapsed);
    }

    public void Commit()
    {
        _activity?.AddEvent(new ActivityEvent("Commit"));
        var watch = Stopwatch.StartNew();
        innerUnitOfWork.Commit();
        watch.Stop();
        collector?.CommitUnitOfWork(watch.Elapsed);
    }

    public void RollBack()
    {
        _activity?.AddEvent(new ActivityEvent("RollBack"));
        _activity?.SetStatus(ActivityStatusCode.Error, "Unit of Work Rolled Back");
        var watch = Stopwatch.StartNew();
        innerUnitOfWork.RollBack();
        watch.Stop();
        collector?.RollbackUnitOfWork(watch.Elapsed);
    }

    public void Dispose()
    {
        _activity?.AddEvent(new ActivityEvent("Dispose"));
        innerUnitOfWork.Dispose();
        collector?.DisposeUnitOfWork();
        _activity?.Dispose();
        _activity = null;
    }
}