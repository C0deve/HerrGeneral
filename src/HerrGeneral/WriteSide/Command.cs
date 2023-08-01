namespace HerrGeneral.WriteSide;

public abstract class Command : CommandBase<CommandResultV2>
{
    protected Command()
    {
        
    }
    protected Command(DateTime executionDate) : base(executionDate)
    {
    }
}