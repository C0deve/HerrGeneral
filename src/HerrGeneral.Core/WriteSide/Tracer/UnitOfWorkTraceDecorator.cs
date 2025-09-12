namespace HerrGeneral.Core.WriteSide.Tracer;

/// <summary>
/// Decorator for IUnitOfWork that adds operation tracing via CommandExecutionTracer.
/// This decorator wraps an existing IUnitOfWork instance and traces all operations.
/// </summary>
internal class UnitOfWorkTraceDecorator(IUnitOfWork innerUnitOfWork, CommandExecutionTracer commandExecutionTracer)
    : IUnitOfWork
{
    public void Start()
    {
        commandExecutionTracer.StartUnitOfWork();
        innerUnitOfWork.Start();
    }

    public void Commit()
    {
        innerUnitOfWork.Commit();
        commandExecutionTracer.CommitUnitOfWork();
    }

    public void RollBack()
    {
        innerUnitOfWork.RollBack();
        commandExecutionTracer.RollbackUnitOfWork();
    }

    public void Dispose()
    {
        innerUnitOfWork.Dispose();
        commandExecutionTracer.DisposeUnitOfWork();
    }
}