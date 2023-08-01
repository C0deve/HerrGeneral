namespace HerrGeneral.WriteSide;

public abstract class CreationCommand : CommandBase<CreationResult>
{
    protected CreationCommand()
    {
        
    }
    protected CreationCommand(DateTime executionDate) : base(executionDate)
    {
    }
}