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

        collector?.StartUnitOfWork();
        innerUnitOfWork.Start();
    }

    public void Commit()
    {
        _activity?.AddEvent(new ActivityEvent("Commit"));
        innerUnitOfWork.Commit();
        collector?.CommitUnitOfWork();
    }

    public void RollBack()
    {
        _activity?.AddEvent(new ActivityEvent("RollBack"));
        _activity?.SetStatus(ActivityStatusCode.Error, "Unit of Work Rolled Back");
        innerUnitOfWork.RollBack();
        collector?.RollbackUnitOfWork();
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